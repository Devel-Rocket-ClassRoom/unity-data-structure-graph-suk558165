using System.Collections.Generic;
using UnityEngine;

public class GraphSearch : MonoBehaviour
{
    protected Graph graph; // 탐색 대상 그래프

    // 탐색 결과 경로
    public List<GraphNode> path = new List<GraphNode>();

    // 탐색 전 그래프 초기화
    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    // 기본 실행 - DFS 호출
    public void Run(GraphNode start)
    {
        DFS(start);
    }

    // -------------------------------------------------------
    // DFS (깊이 우선 탐색)
    // Stack(LIFO) 사용 - 연결된 모든 노드 방문
    // -------------------------------------------------------
    public void DFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>(); // 중복 방문 방지
        var stack = new Stack<GraphNode>();     // 탐색 예정 노드 (LIFO)

        // 시작 노드 push, visited 등록
        stack.Push(node);
        visited.Add(node);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();  // 가장 최근에 넣은 노드 꺼냄
            path.Add(currentNode);          // 꺼낼 때 경로에 추가

            foreach (var adjacent in currentNode.adjacents)
            {
                // 방문 불가 또는 이미 방문한 노드는 skip
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                visited.Add(adjacent); // push 전에 visited 등록 (중복 push 방지)
                stack.Push(adjacent);
            }
        }
    }

    // -------------------------------------------------------
    // BFS (너비 우선 탐색)
    // Queue(FIFO) 사용 - 연결된 모든 노드를 가까운 순서부터 방문
    // -------------------------------------------------------
    public void BFS(GraphNode start)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>(); // 중복 방문 방지
        var queue = new Queue<GraphNode>();     // 탐색 예정 노드 (FIFO)

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue(); // 가장 먼저 넣은 노드 꺼냄
            path.Add(currentNode);             // 꺼낼 때 경로에 추가

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                visited.Add(adjacent);
                queue.Enqueue(adjacent);
            }
        }
    }

    // -------------------------------------------------------
    // DFSRecursive (재귀 깊이 우선 탐색)
    // 함수 호출 스택이 Stack 역할 - Stack을 직접 만들지 않음
    // -------------------------------------------------------

    // 외부 진입점 - path 초기화 및 visited 생성 후 재귀 시작
    public void DFSRecursive(GraphNode start)
    {
        path.Clear();
        DFSRecursive(start, new HashSet<GraphNode>());
    }

    // 실제 재귀 함수 - 매 호출마다 한 노드씩 처리
    private void DFSRecursive(GraphNode node, HashSet<GraphNode> visited)
    {
        path.Add(node);    // 경로에 추가
        visited.Add(node); // 현재 노드 방문 등록

        foreach (var adjacent in node.adjacents)
        {
            // 방문 불가 또는 이미 방문한 노드는 skip
            if (!adjacent.CanVisit || visited.Contains(adjacent))
                continue;

            DFSRecursive(adjacent, visited); // 인접 노드로 재귀 호출
        }
        // 함수 종료 시 자동으로 이전 호출 지점으로 복귀 (= Stack의 pop)
    }

    // -------------------------------------------------------
    // PathFindingBFS (경로 탐색 BFS)
    // BFS로 최단 경로만 path에 저장 (방문 전체 X, 경로만 O)
    // -------------------------------------------------------
    public void PathFindingBFS(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        var visited = new HashSet<GraphNode>(); // 중복 방문 방지
        var queue = new Queue<GraphNode>();     // 탐색 예정 노드 (FIFO)

        queue.Enqueue(startNode);
        visited.Add(startNode);

        // 탐색 전 모든 노드 prividus 초기화 (이전 탐색 결과 제거)
        foreach (var node in graph.nodes)
            node.prividus = null;

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            // 목표 노드 도달 시 prividus 역추적으로 최단 경로 구성
            if (currentNode == endNode)
            {
                var node = endNode;
                while (node != null)
                {
                    path.Add(node);
                    node = node.prividus; // 이전 노드로 거슬러 올라감
                }
                path.Reverse(); // start → end 순서로 뒤집기
                return;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                adjacent.prividus = currentNode; // 어디서 왔는지 기록
                visited.Add(adjacent);
                queue.Enqueue(adjacent);
            }
        }
    }

    // -------------------------------------------------------
    // Dijkstra (다익스트라 최단 경로 탐색)
    // 우선순위 큐 사용 - 누적 가중치가 가장 낮은 노드부터 처리
    // -------------------------------------------------------
    public bool Dijkstra(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var pq = new PriorityQueue<GraphNode, int>();

        // 시작점 = 0, 나머지 = ∞ 로 초기화
        var distances = new int[graph.nodes.Length];
        for (int i = 0; i < distances.Length; ++i)
            distances[i] = int.MaxValue;
        distances[startNode.id] = 0;

        pq.Enqueue(startNode, 0);
        visited.Add(startNode);

        bool success = false;
        while (pq.Count > 0)
        {
            var currentNode = pq.Dequeue();
            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                // v.weight = 정점 v를 지나가는 비용 (도착 비용)
                // 시작점 가중치는 포함하지 않으므로 distances[start] = 0
                int newDist = distances[currentNode.id] + adjacent.weight;
                if (newDist < distances[adjacent.id])
                {
                    distances[adjacent.id] = newDist;
                    adjacent.prividus = currentNode; // 어디서 왔는지 기록
                    pq.Enqueue(adjacent, newDist);
                }
            }

            visited.Add(currentNode);
        }

        if (!success) return false;

        // prividus 역추적으로 경로 구성
        var node = endNode;
        while (node != null)
        {
            path.Add(node);
            node = node.prividus;
        }
        path.Reverse(); // start → end 순서로 뒤집기
        return true;
    }

    // -------------------------------------------------------
    // AStar (A* 최단 경로 탐색)
    // 다익스트라 + 휴리스틱 — f(n) = g(n) + h(n) 기준으로 정렬
    // -------------------------------------------------------
    public bool AStar(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var pq = new PriorityQueue<GraphNode, int>();

        // g(n): 시작점 = 0, 나머지 = ∞
        var distances = new int[graph.nodes.Length];
        for (int i = 0; i < distances.Length; ++i)
            distances[i] = int.MaxValue;
        distances[startNode.id] = 0;

        pq.Enqueue(startNode, 0);
        visited.Add(startNode);

        bool success = false;
        while (pq.Count > 0)
        {
            var currentNode = pq.Dequeue();
            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                // g(v) = g(u) + v.weight (정점 통과 비용 누적)
                int newDist = distances[currentNode.id] + adjacent.weight;
                if (newDist < distances[adjacent.id])
                {
                    distances[adjacent.id] = newDist;
                    adjacent.prividus = currentNode; // 어디서 왔는지 기록
                    // f = g + h: 실제 비용 + 목적지까지 예상 비용
                    int h = Heuristic(adjacent, endNode);
                    pq.Enqueue(adjacent, newDist + h);
                }
            }

            visited.Add(currentNode);
        }

        if (!success) return false;

        // prividus 역추적으로 경로 구성
        var node = endNode;
        while (node != null)
        {
            path.Add(node);
            node = node.prividus;
        }
        path.Reverse(); // start → end 순서로 뒤집기
        return true;
    }

    // 맨해튼 거리 휴리스틱 (4방향 그리드용)
    // id → (x, y) 역산: x = id % cols, y = id / cols
    private int Heuristic(GraphNode a, GraphNode b)
    {
        int ax = a.id % graph.cols;
        int ay = a.id / graph.cols;

        int bx = b.id % graph.cols;
        int by = b.id / graph.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }
}

// Unity는 .NET Standard 2.1을 사용하므로 .NET 6의 PriorityQueue가 없음 — 최소 힙으로 직접 구현
public class PriorityQueue<TElement, TPriority> where TPriority : System.IComparable<TPriority>
{
    private readonly List<(TElement Element, TPriority Priority)> _heap = new List<(TElement, TPriority)>();

    public int Count => _heap.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        _heap.Add((element, priority));
        BubbleUp(_heap.Count - 1);
    }

    public TElement Dequeue()
    {
        var top = _heap[0].Element;
        int last = _heap.Count - 1;
        _heap[0] = _heap[last];
        _heap.RemoveAt(last);
        if (_heap.Count > 0) SiftDown(0);
        return top;
    }

    private void BubbleUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_heap[i].Priority.CompareTo(_heap[parent].Priority) >= 0) break;
            Swap(i, parent);
            i = parent;
        }
    }

    private void SiftDown(int i)
    {
        int count = _heap.Count;
        while (true)
        {
            int smallest = i;
            int left = 2 * i + 1, right = 2 * i + 2;
            if (left < count && _heap[left].Priority.CompareTo(_heap[smallest].Priority) < 0) smallest = left;
            if (right < count && _heap[right].Priority.CompareTo(_heap[smallest].Priority) < 0) smallest = right;
            if (smallest == i) break;
            Swap(i, smallest);
            i = smallest;
        }
    }

    private void Swap(int a, int b)
    {
        var tmp = _heap[a];
        _heap[a] = _heap[b];
        _heap[b] = tmp;
    }
}