using System;
using System.Collections;
using Constants;
using UnityEngine;
using UnityEngine.Events;

namespace Bundles.SimplePlatformer2D.Scripts.Dialogue
{
    /// <summary>
    /// Singleton manager for the dialogue system.
    /// Handles dialogue display, typewriter effect, and time pause.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private DialogueUI dialogueUI;

        [Header("Events")]
        public UnityEvent onDialogueStart;
        public UnityEvent onDialogueEnd;

        private DialogueData currentDialogue;
        private int currentParagraphIndex;
        private bool isTyping;
        private bool isDialogueActive;
        private bool ignoreInputThisFrame;
        private Coroutine typewriterCoroutine;
        private Action onDialogueCompleteCallback;

        public bool IsDialogueActive => isDialogueActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            StopTypewriter();
        }

        private void Update()
        {
            if (!isDialogueActive) return;

            // Ignore input on the frame dialogue started (same keypress that triggered interaction)
            if (ignoreInputThisFrame)
            {
                ignoreInputThisFrame = false;
                return;
            }

            // Use unscaledDeltaTime input check since time is paused
            if (Input.GetButtonDown(GameConstants.Input.Interact))
            {
                HandleInput();
            }
        }

        /// <summary>
        /// Starts a dialogue sequence.
        /// </summary>
        /// <param name="dialogue">The dialogue data to display</param>
        /// <param name="onComplete">Optional callback when dialogue ends</param>
        public void StartDialogue(DialogueData dialogue, Action onComplete = null)
        {
            if (dialogue == null || dialogue.ParagraphCount == 0)
            {
                Debug.LogWarning("DialogueManager: Attempted to start empty dialogue");
                onComplete?.Invoke();
                return;
            }

            currentDialogue = dialogue;
            currentParagraphIndex = 0;
            onDialogueCompleteCallback = onComplete;
            isDialogueActive = true;
            ignoreInputThisFrame = true;

            // Pause time
            Time.timeScale = 0f;

            // Show UI
            dialogueUI.Show();
            dialogueUI.SetSpeakerName(dialogue.SpeakerName);

            // Start first paragraph
            DisplayCurrentParagraph();

            onDialogueStart?.Invoke();
        }

        /// <summary>
        /// Ends the current dialogue immediately.
        /// </summary>
        public void EndDialogue()
        {
            if (!isDialogueActive) return;

            StopTypewriter();

            isDialogueActive = false;
            currentDialogue = null;

            // Resume time
            Time.timeScale = 1f;

            // Hide UI
            dialogueUI.Hide();

            onDialogueEnd?.Invoke();
            onDialogueCompleteCallback?.Invoke();
            onDialogueCompleteCallback = null;
        }

        private void HandleInput()
        {
            if (isTyping)
            {
                // Skip typewriter effect
                SkipTypewriter();
            }
            else
            {
                // Move to next paragraph
                NextParagraph();
            }
        }

        private void DisplayCurrentParagraph()
        {
            string text = currentDialogue.GetParagraph(currentParagraphIndex);
            typewriterCoroutine = StartCoroutine(TypewriterEffect(text, currentDialogue.TypewriterSpeed));
        }

        private IEnumerator TypewriterEffect(string text, float speed)
        {
            isTyping = true;
            dialogueUI.SetText("");

            for (int i = 0; i <= text.Length; i++)
            {
                dialogueUI.SetText(text.Substring(0, i));
                // Use unscaledDeltaTime since time is paused
                yield return new WaitForSecondsRealtime(speed);
            }

            isTyping = false;
            typewriterCoroutine = null;
        }

        private void SkipTypewriter()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            // Display full text
            string fullText = currentDialogue.GetParagraph(currentParagraphIndex);
            dialogueUI.SetText(fullText);
            isTyping = false;
        }

        private void StopTypewriter()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            isTyping = false;
        }

        private void NextParagraph()
        {
            currentParagraphIndex++;

            if (currentParagraphIndex >= currentDialogue.ParagraphCount)
            {
                EndDialogue();
            }
            else
            {
                DisplayCurrentParagraph();
            }
        }
    }
}
