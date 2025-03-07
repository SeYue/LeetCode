using System;
using System.Collections;
using System.Collections.Generic;

// 邻接表
public class AdjacencyList
{
	public VertexNode[] adjList = new VertexNode[100];
	int vertexCount = 6;

	public void CreateGraph()
	{
		for (int i = 0; i < vertexCount; i++)
		{
			adjList[i] = new VertexNode()
			{
				data = i.ToString()
			};
		}

		AddEdge(0, 1, 2);
		AddEdge(0, 2, 1);
		AddEdge(0, 4, 3);
		AddEdge(4, 3, 5);
		AddEdge(1, 5, 1);
		AddEdge(2, 5, 1);
		AddEdge(5, 4, 1);
	}

	// 添加有向边
	public void AddEdge(int tailVex, int headVex, int weight)
	{
		EdgeListNode edgeNode = new EdgeListNode()
		{
			adjvex = headVex,
			weight = weight
		};
		adjList[tailVex].AddEdgeNode(edgeNode);
		adjList[headVex].inCount++;
	}

	// 添加无向边
	public void AddUndirectedEdge(int index1, int index2, int _weight = 1)
	{
		EdgeListNode edgeNode1 = new EdgeListNode()
		{
			adjvex = index2,
			weight = _weight
		};
		adjList[index1].AddEdgeNode(edgeNode1);

		EdgeListNode edgeNode2 = new EdgeListNode()
		{
			adjvex = index1,
			weight = _weight
		};
		adjList[index2].AddEdgeNode(edgeNode2);
	}

	// 深度优先递归
	bool[] visited;

	void DFS(int i)
	{
		visited[i] = true;
		Console.WriteLine($"数组{i}");
		EdgeListNode node = adjList[i].firstEdgeNode;
		while (node != null)
		{
			if (!visited[node.adjvex])
				DFS(node.adjvex);
			node = node.next;
		}
	}

	// 深度优先遍历
	public void DFSTraverse()
	{
		visited = new bool[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			if (!visited[i])
				DFS(i);
		}
	}

	// 广度优先
	public void BFS()
	{
		visited = new bool[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			if (!visited[i])
			{
				visited[i] = true;
				Console.WriteLine($"顶点:{i}");

				VertexNode node = adjList[i];
				if (node != null)
				{
					EdgeListNode edgeNode = node.firstEdgeNode;
					while (edgeNode != null)
					{
						if (!visited[edgeNode.adjvex])
						{
							visited[edgeNode.adjvex] = true;
							Console.WriteLine($"顶点1:{edgeNode.adjvex}");
							edgeNode = edgeNode.next;
						}
					}
				}
			}
		}
	}

	// 拓扑排序,若GL无回路，则输出拓扑排序序列并返回1，若有回路则返回0
	public int TopologicalSort()
	{
		int gettop;
		int count = 0;
		Stack<int> stack = new Stack<int>();    // 建栈将入度为0的顶点入栈

		// 将入度为0的顶点入栈
		for (int i = 0; i < vertexCount; i++)
		{
			if (adjList[i].inCount == 0)
			{
				stack.Push(i);
			}
		}

		stack2.Clear(); // 初始化关键路径的栈
		etv = new int[vertexCount];

		Console.WriteLine("拓扑排序:");
		while (stack.Count != 0)
		{
			gettop = stack.Pop();  // 出栈
			Console.WriteLine($"打印此顶点{adjList[gettop].data}");  // 打印此顶点
			count++;    // 统计输出顶点数量

			stack2.Push(gettop);    // 将取出的顶点下标，压入到stack2的栈。

			// 对此顶点弧表遍历
			for (EdgeListNode e = adjList[gettop].firstEdgeNode; e != null; e = e.next)
			{
				int k = e.adjvex;   // 拿到当前弧的头
				if ((--adjList[k].inCount) <= 0)    // 将k号顶点邻接点的入度-1
					stack.Push(k);               // 若为0则入栈,以便下次循环输出

				if (etv[gettop] + e.weight > etv[k])    // 求各顶点时间的最早发生时间
					etv[k] = etv[gettop] + e.weight;
			}
		}

		Console.WriteLine();
		if (count < vertexCount)
			return 0;
		else
			return 1;
	}

	// 关键路径算法
	int[] etv, ltv;
	Stack<int> stack2 = new Stack<int>();   // 存储拓扑序列的栈

	public void CriticalPath()
	{
		TopologicalSort();
		ltv = new int[vertexCount];

		int max = int.MinValue;
		foreach (var i in etv)
			if (i > max)
				max = i;
		for (int i = 0; i < vertexCount; i++)
			ltv[i] = max;

		// 逆拓扑排序,求出每个顶点的最晚开始时间
		while (stack2.Count > 0)
		{
			int gettop = stack2.Pop();
			for (EdgeListNode e = adjList[gettop].firstEdgeNode; e != null; e = e.next)
			{
				int k = e.adjvex;   // 顶点下标
				if (ltv[k] - e.weight < ltv[gettop])    // 求顶点的最晚发生时间,用的是弧头-弧权重的算法，如果得到的弧长小于当前顶点的最晚发生时间，那么就更新数组。
					ltv[gettop] = ltv[k] - e.weight;
			}
		}

		for (int j = 0; j < vertexCount; j++)
		{
			for (EdgeListNode e = adjList[j].firstEdgeNode; e != null; e = e.next)
			{
				int k = e.adjvex;
				int ete = etv[j];           // 活动最早发生时间
				int lte = ltv[k] - e.weight;// 活动最晚发生时间
				if (ete == lte)
				{
					Console.WriteLine($"<{adjList[j].data} - {adjList[k].data}> length:{e.weight}");
				}
			}
		}
	}
}

// 顶点表结点
public class VertexNode
{
	public int inCount;     // 入度域
	public string data;
	public EdgeListNode firstEdgeNode;

	public void AddEdgeNode(EdgeListNode node)
	{
		if (firstEdgeNode == null)
		{
			firstEdgeNode = node;
		}
		else
		{
			node.next = firstEdgeNode;
			firstEdgeNode = node;
		}
	}
}

// 边表结点
public class EdgeListNode
{
	public int adjvex;
	public int weight;
	public EdgeListNode next;
}

public class AdjacencyListTest
{
	public static void Test()
	{
		AdjacencyList graph = new AdjacencyList();
		graph.CreateGraph();
		graph.CriticalPath();
	}
}
