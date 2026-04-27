using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UiGraphNode : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    private GraphNode graphNode;

    public void Reset()
    {
        SetColor(graphNode.CanVisit ? Color.white : Color.grey);
        SetText($"ID: {graphNode.id}\n Weight: {graphNode.weight}\n Path: -");
    }

    public void SetNode(GraphNode node)
    {
        graphNode = node;
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void SetText(string text)
    {
       this.text.text = text;
    }
}

