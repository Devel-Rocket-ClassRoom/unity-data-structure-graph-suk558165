using System.Collections.Generic;

public class PathFinder
{
    public List<Tile> AStar(Map map, Tile startNode, Tile endNode)
    {
        // 이전 탐색의 previous 포인터 초기화 (경로 재탐색 시 오염 방지)
        foreach (var tile in map.tiles)
            tile.previous = null;

        var path = new List<Tile>();
        var pq = new PriorityQueue<Tile, int>(Comparer<int>.Default);  // 우선순위 큐 (비용 낮은 순)
        var visited = new HashSet<Tile>();                              // 방문 완료 타일
        int[] dist = new int[map.tiles.Length];                        // 시작점에서 각 타일까지 최소 비용

        // 모든 거리를 무한대로 초기화
        for (int i = 0; i < dist.Length; i++)
            dist[i] = int.MaxValue;

        dist[startNode.id] = 0;
        pq.Enqueue(startNode, Heuristic(map, startNode, endNode));  // 시작 타일 추가

        while (pq.Count > 0)
        {
            var currentNode = pq.Dequeue();
            if (!visited.Add(currentNode)) continue;  // 이미 방문한 타일이면 스킵

            // 목적지 도달 시 previous 포인터로 경로 역추적
            if (ReferenceEquals(currentNode, endNode))
            {
                var preNode = currentNode;
                path.Add(preNode);
                while (preNode.previous != null)
                {
                    path.Add(preNode.previous);
                    preNode = preNode.previous;
                }
                path.Reverse();  // 역추적 결과를 시작→끝 순서로 뒤집기
                break;
            }

            // 인접 타일 탐색
            foreach (var adjacent in currentNode.adjacents)
            {
                if (adjacent == null || !adjacent.CanMove || visited.Contains(adjacent))
                    continue;

                // int.MaxValue인 타일은 통과 불가 (오버플로우 방지)
                if (adjacent.MoveCost == int.MaxValue) continue;

                int newDist = dist[currentNode.id] + adjacent.MoveCost;  // 현재까지 비용 + 이동 비용

                // 더 짧은 경로 발견 시 갱신
                if (newDist < dist[adjacent.id])
                {
                    dist[adjacent.id] = newDist;
                    adjacent.previous = currentNode;                                            // 역추적 포인터 저장
                    pq.Enqueue(adjacent, newDist + Heuristic(map, adjacent, endNode));         // f = g + h
                }
            }
        }
        return path;  // 경로 없으면 빈 리스트 반환
    }

    // 휴리스틱: 맨해튼 거리 (대각선 이동 없으므로 최적)
    private int Heuristic(Map map, Tile a, Tile b)
    {
        int ax = a.id % map.cols;
        int ay = a.id / map.cols;
        int bx = b.id % map.cols;
        int by = b.id / map.cols;
        return System.Math.Abs(ax - bx) + System.Math.Abs(ay - by);
    }
}
