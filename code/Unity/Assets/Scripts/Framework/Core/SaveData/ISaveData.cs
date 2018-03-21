namespace Snaplingo.SaveData
{
	public interface ISaveData
	{
		string SaveAsJson();

		void LoadFromJson(string json);

		string SaveTag();
	}
}