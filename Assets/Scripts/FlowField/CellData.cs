using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Wargon.ezs;

namespace LD54.Pathfinding {
    public struct CellData
    {
        public float3 worldPos;
        public int2 gridIndex;
        public byte cost;
        public ushort bestCost;
        public int2 bestDirection;
    }
    public struct DestinationCellData
    {
        public int2 destinationIndex;
    }
    public struct EntityMovementData
    {
        public float destinationMoveSpeed;
        public bool destinationReached;
    }
    
    public struct FlowFieldControllerData
    {
        public int2 gridSize;
        public float cellRadius;
    }
    public struct FlowFieldData
    { 
        public int2 gridSize;
        public float cellRadius;
        public float3 clickedPos;
    }
    public struct CalculateFlowFieldTag { }
    public struct CompleteFlowFieldTag  { }
    public struct EntityBufferElement {
	    public Roguelike.Physics2D.BufferInt256 buffer;
    }
    public class GridDirection
    {
        public readonly int2 Vector;

        private GridDirection(int x, int y)
        {
            Vector = new int2(x, y);
        }

        public static implicit operator int2(GridDirection direction)
        {
            return direction.Vector;
        }

        public static GridDirection GetDirectionFromV2I(int2 vector)
        {
            return CardinalAndIntercardinalDirections.DefaultIfEmpty(None).FirstOrDefault(direction => Equals(direction, vector));
        }

        public static readonly GridDirection None = new GridDirection(0, 0);
        public static readonly GridDirection North = new GridDirection(0, 1);
        public static readonly GridDirection South = new GridDirection(0, -1);
        public static readonly GridDirection East = new GridDirection(1, 0);
        public static readonly GridDirection West = new GridDirection(-1, 0);
        public static readonly GridDirection NorthEast = new GridDirection(1, 1);
        public static readonly GridDirection NorthWest = new GridDirection(-1, 1);
        public static readonly GridDirection SouthEast = new GridDirection(1, -1);
        public static readonly GridDirection SouthWest = new GridDirection(-1, -1);

        public static readonly List<GridDirection> CardinalDirections = new List<GridDirection>
        {
            North,
            East,
            South,
            West
        };

        public static readonly List<GridDirection> CardinalAndIntercardinalDirections = new List<GridDirection>
        {
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        };

        public static readonly List<GridDirection> AllDirections = new List<GridDirection>
        {
            None,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        };
    }
    public static class FlowFieldHelper
    {
	    public static void GetNeighborIndices(int2 originIndex, IEnumerable<GridDirection> directions, int2 gridSize, ref NativeList<int2> results)
	    {
		    foreach (int2 curDirection in directions)
		    {
			    int2 neighborIndex = GetIndexAtRelativePosition(originIndex, curDirection, gridSize);
				
			    if (neighborIndex.x >= 0)
			    {
				    results.Add(neighborIndex);
			    }
		    }
	    }

	    private static int2 GetIndexAtRelativePosition(int2 originPos, int2 relativePos, int2 gridSize)
	    {
			
		    int2 finalPos = originPos + relativePos;
		    if (finalPos.x < 0 || finalPos.x >= gridSize.x || finalPos.y < 0 || finalPos.y >= gridSize.y)
		    {
			    return new int2(-1, -1);
		    }
		    else
		    {
			    return finalPos;
		    }
	    }

	    public static int ToFlatIndex(int2 index2D, int height)
	    {
		    return height * index2D.x + index2D.y;
	    }

	    public static int2 GetCellIndexFromWorldPos(float3 worldPos, int2 gridSize, float cellDiameter)
	    {
		    float percentX = worldPos.x / (gridSize.x * cellDiameter);
		    float percentY = worldPos.z / (gridSize.y * cellDiameter);

		    percentX = math.clamp(percentX, 0f, 1f);
		    percentY = math.clamp(percentY, 0f, 1f);
            
		    int2 cellIndex = new int2
		    {
			    x = math.clamp((int) math.floor((gridSize.x) * percentX), 0, gridSize.x - 1),
			    y = math.clamp((int) math.floor((gridSize.y) * percentY), 0, gridSize.y - 1)
		    };

		    return cellIndex;
	    }
    }
    
    public partial class CalculateFlowFieldSystem : UpdateSystem {
	    private Pool<CellData> cellsData;
		public override void Update() {
			entities.Each((Entity entity, EntityBufferElement buffer, CalculateFlowFieldTag calculateFlowFieldTag,
				DestinationCellData destinationCellData, FlowFieldData flowFieldData) => {
				
				entity.Remove<CalculateFlowFieldTag>();

				NativeArray<CellData> cellDataContainer = new NativeArray<CellData>(buffer.buffer.Count, Allocator.Temp);
				
				int2 gridSize = flowFieldData.gridSize;

				for (int i = 0; i < buffer.buffer.Count; i++) {
					cellDataContainer[i] = cellsData.Get(buffer.buffer[i]);
				}

				int flatDestinationIndex = FlowFieldHelper.ToFlatIndex(destinationCellData.destinationIndex, gridSize.y);
				CellData destinationCell = cellDataContainer[flatDestinationIndex];
				destinationCell.cost = 0;
				destinationCell.bestCost = 0;
				cellDataContainer[flatDestinationIndex] = destinationCell;

				NativeQueue<int2> indicesToCheck = new NativeQueue<int2>(Allocator.Temp);
				NativeList<int2> neighborIndices = new NativeList<int2>(Allocator.Temp);

				indicesToCheck.Enqueue(destinationCellData.destinationIndex);
				
				// Integration Field
				while (indicesToCheck.Count > 0)
				{
					int2 cellIndex = indicesToCheck.Dequeue();
					int cellFlatIndex = FlowFieldHelper.ToFlatIndex(cellIndex, gridSize.y);
					CellData curCellData = cellDataContainer[cellFlatIndex];
					neighborIndices.Clear();
					FlowFieldHelper.GetNeighborIndices(cellIndex, GridDirection.CardinalDirections, gridSize, ref neighborIndices);
					foreach (int2 neighborIndex in neighborIndices)
					{
						int flatNeighborIndex = FlowFieldHelper.ToFlatIndex(neighborIndex, gridSize.y);
						CellData neighborCellData = cellDataContainer[flatNeighborIndex];
						if (neighborCellData.cost == byte.MaxValue)
						{
							continue;
						}

						if (neighborCellData.cost + curCellData.bestCost < neighborCellData.bestCost)
						{
							neighborCellData.bestCost = (ushort) (neighborCellData.cost + curCellData.bestCost);
							cellDataContainer[flatNeighborIndex] = neighborCellData;
							indicesToCheck.Enqueue(neighborIndex);
						}
					}
				}

				// Flow Field
				for (int i = 0; i < cellDataContainer.Length; i++)
				{
					CellData curCullData = cellDataContainer[i];
					neighborIndices.Clear();
					FlowFieldHelper.GetNeighborIndices(curCullData.gridIndex, GridDirection.AllDirections, gridSize, ref neighborIndices);
					ushort bestCost = curCullData.bestCost;
					int2 bestDirection = int2.zero;
					foreach (int2 neighborIndex in neighborIndices)
					{
						int flatNeighborIndex = FlowFieldHelper.ToFlatIndex(neighborIndex, gridSize.y);
						CellData neighborCellData = cellDataContainer[flatNeighborIndex];
						if (neighborCellData.bestCost < bestCost)
						{
							bestCost = neighborCellData.bestCost;
							bestDirection = neighborCellData.gridIndex - curCullData.gridIndex;
						}
					}
					curCullData.bestDirection = bestDirection;
					cellDataContainer[i] = curCullData;
				}


				for (int i = 0; i < buffer.buffer.Count; i++) {
					var e = world.GetEntity(buffer.buffer[i]);
					e.Add(cellDataContainer[i]);
				}

				neighborIndices.Dispose();
				cellDataContainer.Dispose();
				indicesToCheck.Dispose();
				
				entity.Add(new CompleteFlowFieldTag());
				
			});
		}
	}
    public partial class CompleteFlowFieldSystem : UpdateSystem
    {
	    public override void Update()
	    {
		    entities.Each(
			    (Entity entity, CompleteFlowFieldTag completeFlowFieldTag, FlowFieldData flowFieldData) => {
				    entity.Remove<CompleteFlowFieldTag>();
			    });
		    
		    
		    // Entities.ForEach((Entity entity, in CompleteFlowFieldTag completeFlowFieldTag, in FlowFieldData flowFieldData) =>
		    // {
			   //  commandBuffer.RemoveComponent<CompleteFlowFieldTag>(entity);
			   //  EntityMovementSystem.instance.SetMovementValues();
		    // }).Run();
	    }
    }
}

