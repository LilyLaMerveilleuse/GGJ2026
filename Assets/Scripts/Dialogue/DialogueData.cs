using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts.Dialogue
{
    /// <summary>
    /// ScriptableObject containing dialogue data.
    /// Create new dialogues via Assets > Create > Dialogue > Dialogue Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [Header("Dialogue Content")]
        [SerializeField, TextArea(3, 10)]
        private string[] paragraphs;

        [Header("Settings")]
        [SerializeField] private float typewriterSpeed = 0.05f;
        [SerializeField] private string speakerName;

        public string[] Paragraphs => paragraphs;
        public float TypewriterSpeed => typewriterSpeed;
        public string SpeakerName => speakerName;

        public int ParagraphCount => paragraphs?.Length ?? 0;

        public string GetParagraph(int index)
        {
            if (paragraphs == null || index < 0 || index >= paragraphs.Length)
                return string.Empty;
            return paragraphs[index];
        }
    }
}
