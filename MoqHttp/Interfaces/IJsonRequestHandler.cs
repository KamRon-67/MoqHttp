using Newtonsoft.Json.Linq;

namespace MoqHttp.Interfaces
{
	public interface IJsonRequestHandler
	{
		bool ValidatingWhenReadingJSON(string json);
		JObject ReadJSONFromFile(string path);
	}
}