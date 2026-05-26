using CoreData;
using MeshGeneration.Interfaces;
using MeshGeneration.Mesher;
using MeshGeneration.Utils;
using RuntimeData;
using RuntimeData.Factories;
using UnityEngine;

namespace MeshGeneration.Greedy
{
	public class GreedyCombinedMesher : ICombinedMesher
	{
		private const int SIZE = ObjectRuntime.CHUNK_SIZE;

		private readonly FaceInfo[,] _visualMask = new FaceInfo[SIZE, SIZE];
		private readonly bool[,] _colliderMask = new bool[SIZE, SIZE];
		private readonly bool[,] _used = new bool[SIZE, SIZE];
		private readonly Vector3[] _quad = new Vector3[4];

		private static readonly VoxelRenderInfo[] InfoByType;
		private static readonly bool[] HasColliderByType;

		static GreedyCombinedMesher()
		{
			InfoByType = new VoxelRenderInfo[256];
			HasColliderByType = new bool[256];

			foreach (var kv in VoxelRegistry.Info)
			{
				InfoByType[kv.Key] = kv.Value;
				HasColliderByType[kv.Key] = kv.Value.HasCollider;
			}
		}

		private static bool HasCollider(VoxelData? v)
			=> v.HasValue && HasColliderByType[v.Value.Type];

		public VoxelTotalResult Build(VoxelData?[] grid, ChunkNeighbors nb, VoxelBuildMode buildMode)
		{
			var opaque = new MeshBuilder();
			var transparent = new MeshBuilder();

			bool buildCollider = buildMode != VoxelBuildMode.VisualOnly;
			bool colliderAllVoxels = buildMode == VoxelBuildMode.ColliderAll;

			MeshBuilder colliderBuilder = buildCollider ? new MeshBuilder() : null;

			for (int face = 0; face < 6; face++)
			{
				BuildDirection(
					grid,
					nb,
					opaque,
					transparent,
					colliderBuilder,
					buildCollider,
					colliderAllVoxels,
					face,
					VoxelFaces.DX[face],
					VoxelFaces.DY[face],
					VoxelFaces.DZ[face]);
			}

			var visual = new VoxelVisualMeshResult
			{
				OpaqueMesh = opaque.Build(),
				TransparentMesh = transparent.Build()
			};

			Mesh colliderMesh = null;
			if (buildCollider)
			{
				colliderMesh = colliderBuilder.Build();
				colliderMesh.RecalculateBounds();
			}

			return new VoxelTotalResult
			{
				Visual = visual,
				Collider = colliderMesh
			};
		}

		private void BuildDirection(
			VoxelData?[] grid,
			ChunkNeighbors nb,
			MeshBuilder opaque,
			MeshBuilder transparent,
			MeshBuilder collider,
			bool buildCollider,
			bool colliderAllVoxels,
			int faceIndex,
			int dx,
			int dy,
			int dz)
		{
			for (int w = 0; w < SIZE; w++)
			{
				// 1) заполняем маски
				for (int u = 0; u < SIZE; u++)
				for (int v = 0; v < SIZE; v++)
				{
					int x, y, z;
					MapCoords(faceIndex, w, u, v, out x, out y, out z);

					var cur = NeighborUtils.Get(x, y, z, grid, nb);
					if (!cur.HasValue)
					{
						_visualMask[u, v].IsSet = false;
						_colliderMask[u, v] = false;
						continue;
					}

					var neigh = NeighborUtils.Get(x + dx, y + dy, z + dz, grid, nb);

					bool showFace = VoxelFaceUtils.ShouldShowFace(cur, neigh);

					// визуал
					if (!showFace)
					{
						_visualMask[u, v].IsSet = false;
					}
					else
					{
						ref readonly var info = ref InfoByType[cur.Value.Type];
						_visualMask[u, v] = new FaceInfo
						{
							IsSet = true,
							IsTransparent = info.IsTransparent,
							Submesh = info.SubmeshIndex,
							Color = cur.Value.Color,
							Type = cur.Value.Type
						};
					}

					// коллайдер
					if (buildCollider)
					{
						bool curCollider = colliderAllVoxels || HasCollider(cur);
						_colliderMask[u, v] = curCollider && showFace;
					}
					else
					{
						_colliderMask[u, v] = false;
					}
				}

				// 2) greedy визуала
				GreedyVisualLayer(_visualMask, w, dx, dy, dz, opaque, transparent);

				// 3) greedy коллайдера
				if (buildCollider)
					GreedyColliderLayer(_colliderMask, w, dx, dy, dz, collider);
			}
		}

		private void GreedyVisualLayer(
			FaceInfo[,] mask,
			int w,
			int dx,
			int dy,
			int dz,
			MeshBuilder opaque,
			MeshBuilder transparent)
		{
			System.Array.Clear(_used, 0, _used.Length);

			for (int u = 0; u < SIZE; u++)
			for (int v = 0; v < SIZE; v++)
			{
				if (!mask[u, v].IsSet || _used[u, v])
					continue;

				var fi = mask[u, v];

				int width = 1;
				while (u + width < SIZE && Same(mask[u + width, v], fi) && !_used[u + width, v])
					width++;

				int height = 1;
				bool stop = false;
				while (v + height < SIZE && !stop)
				{
					for (int k = 0; k < width; k++)
					{
						if (!Same(mask[u + k, v + height], fi) || _used[u + k, v + height])
						{
							stop = true;
							break;
						}
					}

					if (!stop) height++;
				}

				for (int du = 0; du < width; du++)
				for (int dv = 0; dv < height; dv++)
					_used[u + du, v + dv] = true;

				AddVisualQuad(u, v, width, height, w, dx, dy, dz, fi, opaque, transparent);
			}
		}

		private void GreedyColliderLayer(
			bool[,] mask,
			int w,
			int dx,
			int dy,
			int dz,
			MeshBuilder collider)
		{
			System.Array.Clear(_used, 0, _used.Length);

			for (int u = 0; u < SIZE; u++)
			for (int v = 0; v < SIZE; v++)
			{
				if (!mask[u, v] || _used[u, v])
					continue;

				int width = 1;
				while (u + width < SIZE && mask[u + width, v] && !_used[u + width, v])
					width++;

				int height = 1;
				bool stop = false;
				while (v + height < SIZE && !stop)
				{
					for (int k = 0; k < width; k++)
					{
						if (!mask[u + k, v + height] || _used[u + k, v + height])
						{
							stop = true;
							break;
						}
					}

					if (!stop) height++;
				}

				for (int du = 0; du < width; du++)
				for (int dv = 0; dv < height; dv++)
					_used[u + du, v + dv] = true;

				AddColliderQuad(u, v, width, height, w, dx, dy, dz, collider);
			}
		}

		private void AddVisualQuad(
			int u,
			int v,
			int width,
			int height,
			int w,
			int dx,
			int dy,
			int dz,
			FaceInfo fi,
			MeshBuilder opaque,
			MeshBuilder transparent)
		{
			ComputeQuad(u, v, width, height, w, dx, dy, dz, _quad);

			var e20 = _quad[2] - _quad[0];
			var e10 = _quad[1] - _quad[0];
			var cross = Vector3.Cross(e20, e10);
			Vector3 dir = new(dx, dy, dz);

			if (Vector3.Dot(cross, dir) < 0f)
				(_quad[1], _quad[3]) = (_quad[3], _quad[1]);

			var normal = dir;

			if (!fi.IsTransparent)
				opaque.AddQuad(_quad, normal, fi.Submesh, fi.Color);
			else
				transparent.AddQuad(_quad, normal, fi.Submesh, fi.Color);
		}

		private void AddColliderQuad(
			int u,
			int v,
			int width,
			int height,
			int w,
			int dx,
			int dy,
			int dz,
			MeshBuilder collider)
		{
			ComputeQuad(u, v, width, height, w, dx, dy, dz, _quad);
			var e20 = _quad[2] - _quad[0];
			var e10 = _quad[1] - _quad[0];
			var cross = Vector3.Cross(e20, e10);
			Vector3 dir = new(dx, dy, dz);

			if (Vector3.Dot(cross, dir) < 0f)
				(_quad[1], _quad[3]) = (_quad[3], _quad[1]);
			collider.AddQuad(_quad, Vector3.zero, 0, default);
		}

		private static void MapCoords(int face, int w, int u, int v, out int x, out int y, out int z)
		{
			switch (face)
			{
				case 0:
				case 1:
					x = w;
					y = u;
					z = v;
					return;
				case 2:
				case 3:
					x = u;
					y = w;
					z = v;
					return;
				default:
					x = u;
					y = v;
					z = w;
					return;
			}
		}

		private static bool Same(FaceInfo a, FaceInfo b)
		{
			if (!a.IsSet || !b.IsSet) return false;
			return a.Type == b.Type &&
			       a.IsTransparent == b.IsTransparent &&
			       a.Submesh == b.Submesh &&
			       a.Color.Equals(b.Color);
		}

		private static void ComputeQuad(
			int u,
			int v,
			int width,
			int height,
			int w,
			int dx,
			int dy,
			int dz,
			Vector3[] quad)
		{
			float ox, oy, oz;
			float dux, duy, duz;
			float dvx, dvy, dvz;

			if (dx != 0)
			{
				ox = w + (dx > 0 ? 1 : 0);
				oy = u;
				oz = v;

				dux = 0;
				duy = width;
				duz = 0;
				dvx = 0;
				dvy = 0;
				dvz = height;
			}
			else if (dy != 0)
			{
				ox = u;
				oy = w + (dy > 0 ? 1 : 0);
				oz = v;

				dux = width;
				duy = 0;
				duz = 0;
				dvx = 0;
				dvy = 0;
				dvz = height;
			}
			else
			{
				ox = u;
				oy = v;
				oz = w + (dz > 0 ? 1 : 0);

				dux = width;
				duy = 0;
				duz = 0;
				dvx = 0;
				dvy = height;
				dvz = 0;
			}

			quad[0] = new Vector3(ox, oy, oz);
			quad[1] = new Vector3(ox + dux, oy + duy, oz + duz);
			quad[2] = new Vector3(ox + dux + dvx, oy + duy + dvy, oz + duz + dvz);
			quad[3] = new Vector3(ox + dvx, oy + dvy, oz + dvz);
		}

		private struct FaceInfo
		{
			public bool IsSet;
			public bool IsTransparent;
			public int Submesh;
			public Color32 Color;
			public byte Type;
		}
	}
}