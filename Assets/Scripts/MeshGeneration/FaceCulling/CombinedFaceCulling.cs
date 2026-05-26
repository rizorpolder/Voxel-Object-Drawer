using CoreData;
using MeshGeneration.Interfaces;
using MeshGeneration.Mesher;
using MeshGeneration.Utils;
using RuntimeData.Factories;
using UnityEngine;

namespace MeshGeneration.FaceCulling
{
	public class FaceCullingCombinedMesher : ICombinedMesher
	{
		private static readonly VoxelRenderInfo[] InfoByType;
		private static readonly bool[] HasColliderByType;

		private static readonly Vector3[] Normals =
		{
			new(1, 0, 0),
			new(-1, 0, 0),
			new(0, 1, 0),
			new(0, -1, 0),
			new(0, 0, 1),
			new(0, 0, -1)
		};

		static FaceCullingCombinedMesher()
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

		private static bool IsAir(VoxelData? v)
			=> !v.HasValue || v.Value.Type == 0;

		public VoxelTotalResult Build(VoxelData?[] grid, ChunkNeighbors nb, VoxelBuildMode buildMode)
		{
			int S = VoxelFaces.S;

			var opaque = new MeshBuilder();
			var transparent = new MeshBuilder();

			bool buildCollider = buildMode != VoxelBuildMode.VisualOnly;

			MeshBuilder colliderBuilder = buildCollider ? new MeshBuilder() : null;

			var fv = VoxelFaces.FACE_VERTS;
			var dx = VoxelFaces.DX;
			var dy = VoxelFaces.DY;
			var dz = VoxelFaces.DZ;

			var quad = new Vector3[4];

			for (int x = 0; x < S; x++)
			{
				int baseX = x * S * S;

				for (int y = 0; y < S; y++)
				{
					int baseXY = baseX + y * S;

					for (int z = 0; z < S; z++)
					{
						var voxel = grid[baseXY + z];
						if (!voxel.HasValue)
							continue;

						byte type = voxel.Value.Type;
						ref readonly var info = ref InfoByType[type];

						bool curCollider =
							buildCollider &&
							(buildMode == VoxelBuildMode.ColliderAll || HasCollider(voxel));

						for (int d = 0; d < 6; d++)
						{
							int nx = x + dx[d];
							int ny = y + dy[d];
							int nz = z + dz[d];

							var neigh = NeighborUtils.Get(nx, ny, nz, grid, nb);

							// ---------- ВИЗУАЛ ----------
							if (VoxelFaceUtils.ShouldShowFace(voxel, neigh))
							{
								var src = fv[d];

								quad[0].Set(src[0].x + x, src[0].y + y, src[0].z + z);
								quad[1].Set(src[1].x + x, src[1].y + y, src[1].z + z);
								quad[2].Set(src[2].x + x, src[2].y + y, src[2].z + z);
								quad[3].Set(src[3].x + x, src[3].y + y, src[3].z + z);

								Vector3 normal = Normals[d];

								if (!info.IsTransparent)
									opaque.AddQuad(quad, normal, info.SubmeshIndex, voxel.Value.Color);
								else
									transparent.AddQuad(quad, normal, info.SubmeshIndex, voxel.Value.Color);
							}

							// ---------- КОЛЛАЙДЕР ----------
							if (!curCollider)
								continue;

							if (!VoxelFaceUtils.ShouldShowFace(voxel, neigh))
								continue;


							var srcC = fv[d];

							quad[0].Set(srcC[0].x + x, srcC[0].y + y, srcC[0].z + z);
							quad[1].Set(srcC[1].x + x, srcC[1].y + y, srcC[1].z + z);
							quad[2].Set(srcC[2].x + x, srcC[2].y + y, srcC[2].z + z);
							quad[3].Set(srcC[3].x + x, srcC[3].y + y, srcC[3].z + z);

							colliderBuilder.AddQuad(
								quad,
								Vector3.zero,
								0,
								default
							);
						}
					}
				}
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
	}
}