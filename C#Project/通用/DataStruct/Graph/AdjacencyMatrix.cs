using System;
using System.Collections.Generic;

// 邻接矩阵
public class AdjacencyMatrix
{
	public const int maxVertexs = 10;
	public char[] vertexs = new char[maxVertexs];  // 顶点表
	public int[,] arcs = new int[maxVertexs, maxVertexs];    // 邻接矩阵

	public int vertexCount, edgesCount;

	// 建立无向图的邻接矩阵
	public void CreateMGraph(int numNodes)
	{
		vertexCount = numNodes;

		int i, j, k, w;
		Console.WriteLine("输入顶点数和边数");

		for (i = 0; i < numNodes; i++)
		{
			vertexs[i] = (char)i;// Console.ReadLine()[0];
		}
		for (i = 0; i < maxVertexs; i++)
		{
			for (j = 0; j < maxVertexs; j++)
			{
				arcs[i, j] = int.MaxValue;
			}
		}
		AddEdge(0, 1, 3);
		AddEdge(0, 2, 2);
		AddEdge(0, 3, 1);
		AddEdge(1, 2, 1);
		AddEdge(3, 2, 4);
		AddEdge(0, 4, 10);
	}

	void AddEdge(int begin, int end, int weight)
	{
		arcs[begin, end] = weight;
		arcs[end, begin] = weight;
		edgesCount++;
	}

	// 深度优先递归算法
	bool[] visited;

	public void DFSTraverse()
	{
		visited = new bool[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			if (!visited[i])
				DFS(i);
		}
	}

	void DFS(int i)
	{
		visited[i] = true;
		Console.WriteLine($"顶点:{i}");
		for (int j = 0; j < vertexCount; j++)
		{
			if (arcs[i, j] == 1 && !visited[j])
			{
				DFS(j);
			}
		}
	}

	// 广度优先
	public void BFS()
	{
		visited = new bool[vertexCount];
		Queue<int> waitCheckedNode = new Queue<int>();
		waitCheckedNode.Enqueue(0);
		for (int j = 0; j < vertexCount; j++)
		{
			if (!visited[j])
			{
				visited[j] = true;
				waitCheckedNode.Enqueue(j);
				Console.WriteLine($"顶点1:{j}");

				while (waitCheckedNode.Count > 0)
				{
					int index = waitCheckedNode.Dequeue();
					for (int i = 0; i < vertexCount; i++)
					{
						if (arcs[index, i] == 1 && !visited[i])
						{
							visited[i] = true;
							Console.WriteLine($"顶点2:{i}");
							waitCheckedNode.Enqueue(i);
						}
					}
				}
			}
		}
	}

	// 最小生成树,普利姆算法
	public void MiniSpanTree_Prim()
	{
		int min, i, j, k;
		int[] adjvex = new int[vertexCount];    // 保存相关顶点的权值下标
		int[] lowcost = new int[vertexCount];   // 记录了每个顶点的权重
		adjvex[0] = 0;
		lowcost[0] = 0;

		for (i = 1; i < vertexCount; i++)
		{
			lowcost[i] = arcs[0, i];
			adjvex[i] = 0;
		}

		for (i = 1; i < vertexCount; i++)
		{
			min = int.MaxValue;
			j = 1;
			k = 0;

			while (j < vertexCount)
			{
				if (lowcost[j] != 0 && lowcost[j] < min)
				{
					min = lowcost[j];
					k = j;
				}
				j++;
			}

			Console.WriteLine($"顶点:{k},权重:{min}");
			lowcost[k] = 0;

			for (j = 1; j < vertexCount; j++)
			{
				if (lowcost[j] != 0 && arcs[k, j] < lowcost[j])
				{
					lowcost[j] = arcs[k, j];
					adjvex[j] = k;
				}
			}
		}
	}

	// 最小生成树,克鲁斯卡尔算法
	public void MiniSpanTree_Kruskal()
	{
		// 边集数组,记录了图中所有的边的信息
		EdgeArray[] edges = new EdgeArray[edgesCount];
		// 定义一组数组,用来判断最小生成树中，边与边是否形成环路
		int[] parent = new int[vertexCount];

		// 将邻接矩阵转换为边集数组
		int k = 0;
		for (int i = 0; i < edgesCount; i++)
		{
			for (int j = i + 1; j < edgesCount; j++)
			{
				if (arcs[i, j] != int.MaxValue)
				{
					edges[k] = new EdgeArray()
					{
						begin = i,
						end = j,
						weight = arcs[i, j]
					};
					k++;
				}
			}
		}

		// 对边集数组排序，权重从小到大
		for (int a = 0; a < edgesCount; a++)
		{
			for (int b = a + 1; b < edgesCount; b++)
			{
				if (edges[a].weight > edges[b].weight)
				{
					var temp = edges[a];
					edges[a] = edges[b];
					edges[b] = temp;
				}
			}
		}

		// 循环每一条边
		for (int i = 0; i < edgesCount; i++)
		{
			Console.WriteLine($"正在检查第{i}条边,{edges[i].begin},{edges[i].end}:{edges[i].weight}");
			int n = Find(parent, edges[i].begin);
			int m = Find(parent, edges[i].end);

			// 假如n与m不相等,说明此边没有与现有的生成树形成环路
			if (n != m)
			{
				parent[n] = m;
				Console.WriteLine($"{edges[i].begin},{edges[i].end},{edges[i].weight}");
			}
		}
	}

	// 查找连线顶点的尾部下标
	public int Find(int[] parent, int f)
	{
		while (parent[f] > 0)
		{
			f = parent[f];
		}
		return f;
	}

	// 求图中两点之间边的权重之和最短的路径,迪杰斯特拉算法
	public void ShortestPath_Dijkstra(int v0)
	{
		// 用于存储最短路径下标的数组
		int[] Patharc = new int[vertexCount];
		// 用于存储到各点最短路径的权值和
		int[] ShortPathTable = new int[vertexCount];

		// final[w]表示顶点v0至vw的路径是否已经有结果了
		bool[] final = new bool[vertexCount];
		for (int v = 0; v < vertexCount; v++)   // 初始化数据
		{
			final[v] = false;
			ShortPathTable[v] = arcs[v0, v];           // 拿到邻接矩阵第一个顶点的出边表,记录有和顶点v0相连的线的权重
			Patharc[v] = -1;    // 初始化路径数组P为-1
		}

		// 初始化v0到v0的路径
		final[v0] = true;          // 起点到起点不需要求路径
		ShortPathTable[v0] = 0; // 初始化起点路径长度为0

		// 主循环,每次循环求得v0到一个顶点的最短路径,因此v从1开始而不是0开始
		int k = 0;
		for (int v = 1; v < vertexCount; v++)   // 开启主循环,每次求得v0到某个顶点v的最短路径
		{
			// 找到和当前顶点相连的权值最小的边,记录下标和权值
			int min = int.MaxValue;     // 当前所知的离顶点v0最近的距离
			for (int w = 0; w < vertexCount; w++)   // 遍历所有的顶点，找出离v0最近距离的顶点
			{
				if (!final[w] &&            // 当前顶点不在已连接的路径里面
					ShortPathTable[w] < min)    // 当前顶点到其他顶点的距离，如果小于当前最小的距离，就更新顶点下标和最小距离
				{
					k = w;
					min = ShortPathTable[w];
				}
			}

			final[k] = true;   // 将目前找到的最近的顶点置为1

			// 在已经找到的当前顶点最短路径的基础上，对当前顶点的其他边进行计算，得到v0与他们的最短距离
			for (int w = 0; w < vertexCount; w++)   // 修正当前最短路径及距离
			{
				if (!final[w] && (min + arcs[k, w] < ShortPathTable[w]))    // 如果经过v顶点的路径比现在这条路径的长度短
				{
					ShortPathTable[w] = min + arcs[k, w];   // 更新该顶点到v0的最短路径
					Patharc[w] = k; // 更新该顶点的前缀
				}
			}
		}
	}

	// 弗洛伊德算法，求所有顶点到所有顶点的最短距离
	public void ShortestPath_Floyd()
	{
		int[,] Patharc = new int[vertexCount, vertexCount];
		int[,] ShortPathTable = new int[vertexCount, vertexCount];

		for (int v = 0; v < vertexCount; v++)
		{
			for (int w = 0; w < vertexCount; w++)
			{
				Patharc[v, w] = arcs[v, w];
				ShortPathTable[v, w] = w;
			}
		}

		for (int k = 0; k < vertexCount; k++)	// 所有顶点都经过顶点0中转
		{
			for (int v = 0; v < vertexCount; v++)
			{
				for (int w = 0; w < vertexCount; w++)
				{
					// 如果经过下标为k顶点的路径比原两点的路径短
					if (ShortPathTable[v, w] > ShortPathTable[v, k] + ShortPathTable[k, w])
					{
						// 将当前两点间的权值设更小一个
						ShortPathTable[v, w] = ShortPathTable[v, k] + ShortPathTable[k, w];
						Patharc[v, w] = Patharc[v, k];
					}
				}
			}
		}
	}
}

public class AdjacencyMatrixTest
{
	public static void Test()
	{
		AdjacencyMatrix am = new AdjacencyMatrix();
		am.CreateMGraph(5);
		am.MiniSpanTree_Kruskal();
	}
}

// 边集数组的一个结构
public class EdgeArray
{
	public int begin;
	public int end;
	public int weight;
}
