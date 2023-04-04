using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HuggingFace.API {
    public class TextToImageTask : TaskBase<string, Texture2D> {
        public override string taskName => "TextToImage";
        public override string defaultEndpoint => "https://api-inference.huggingface.co/models/runwayml/stable-diffusion-v1-5";

        protected override JObject GetPayload(string input, object context) {
            return new JObject {
                ["inputs"] = input
            };
        }

        protected override bool PostProcess(object raw, string input, object context, out Texture2D response, out string error) {
            response = new Texture2D(2, 2);
            if (response.LoadImage(raw as byte[])) {
                error = null;
                return true;
            } else {
                error = "Failed to load image.";
                return false;
            }
        }
    }
}