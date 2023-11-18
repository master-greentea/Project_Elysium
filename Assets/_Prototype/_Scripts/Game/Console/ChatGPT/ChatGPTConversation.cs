using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Events;

namespace ChatGPTWrapper {

    public class ChatGPTConversation : MonoBehaviour
    {
        [SerializeField] public string _apiKey = null;
        public enum Model {
            ChatGPT,
            Davinci,
            Curie
        }
        [SerializeField] public Model _model = Model.ChatGPT;
        private string _selectedModel = null;
        [SerializeField] private int _maxTokens = 3072;
        [SerializeField] private float _temperature = 0.6f;
        private string _uri;
        private List<(string, string)> _reqHeaders;
        private Requests requests = new Requests();
        private Prompt _prompt;
        private Chat _chat;
        private string _lastUserMsg;
        private string _lastChatGPTMsg;
        [SerializeField] private string _chatbotName = "ChatGPT";
        public string _initialPrompt;
        public UnityStringEvent chatGPTResponse = new UnityStringEvent();
        
        [SerializeField] private bool _debugAPIFromFile;
        
        public void Init()
        {
            // read api key
            if (_debugAPIFromFile)
            {
                string[] apiLines = System.IO.File.ReadAllLines(@"Assets/_Prototype/_Scripts/Game/Console/api_keys.txt");
                _apiKey = apiLines[0];
            }

            _reqHeaders = new List<(string, string)>
            { 
                ("Authorization", $"Bearer {_apiKey}"),
                ("Content-Type", "application/json")
            };
            switch (_model) {
                case Model.ChatGPT:
                    _chat = new Chat(_initialPrompt);
                    _uri = "https://api.openai.com/v1/chat/completions";
                    _selectedModel = "gpt-3.5-turbo";
                    break;
                case Model.Davinci:
                    _prompt = new Prompt(_chatbotName, _initialPrompt);
                    _uri = "https://api.openai.com/v1/completions";
                    _selectedModel = "text-davinci-003";
                    break;
                case Model.Curie:
                    _prompt = new Prompt(_chatbotName, _initialPrompt);
                    _uri = "https://api.openai.com/v1/completions";
                    _selectedModel = "text-curie-001";
                    break;
            }

            Services.ConsoleMenuManager.consoleInitialized = true;
        }

        public void ResetChat(string initialPrompt) {
            switch (_model) {
                case Model.ChatGPT:
                    _chat = new Chat(initialPrompt);
                    break;
                default:
                    _prompt = new Prompt(_chatbotName, initialPrompt);
                    break;
            }
        }

        public void SendToChatGPT(string message)
        {
            _lastUserMsg = message;

            if (_model == Model.ChatGPT) {
                _chat.AppendMessage(Chat.Speaker.User, message);

                ChatGPTReq reqObj = new ChatGPTReq();
                reqObj.model = _selectedModel;
                reqObj.messages = _chat.CurrentChat;
        
                string json = JsonUtility.ToJson(reqObj);



                StartCoroutine(requests.PostReq<ChatGPTRes>(_uri, json, ResolveChatGPT, _reqHeaders));

               

            } else {
                _prompt.AppendText(Prompt.Speaker.User, message);

                GPTReq reqObj = new GPTReq();
                reqObj.model = _selectedModel;
                reqObj.prompt = _prompt.CurrentPrompt;
                reqObj.max_tokens = _maxTokens;
                reqObj.temperature = _temperature;
                string json = JsonUtility.ToJson(reqObj);

                StartCoroutine(requests.PostReq<GPTRes>(_uri, json, ResolveGPT, _reqHeaders));
            }
        }

        private void ResolveChatGPT(ChatGPTRes res)
        {
            _lastChatGPTMsg = res.choices[0].message.content;

            _chat.AppendMessage(Chat.Speaker.ChatGPT, _lastChatGPTMsg);
            chatGPTResponse.Invoke(_lastChatGPTMsg);

            //If the total_tokens over 4096 token limitation, remove the oldest message after initial prompt
			int totalToken = res.usage.total_tokens;
			if (totalToken > 3850)
			{
				_chat.RemoveOldestMessage();
			}
			print("token: " + totalToken);
		}

        private void ResolveGPT(GPTRes res)
        {
            _lastChatGPTMsg = res.choices[0].text
                .TrimStart('\n')
                .Replace("<|im_end|>", "");

            _prompt.AppendText(Prompt.Speaker.Bot, _lastChatGPTMsg);
            chatGPTResponse.Invoke(_lastChatGPTMsg);
        }
    }
}
