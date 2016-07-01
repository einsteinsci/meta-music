namespace MetaMusic
{
	public interface ILoadingText
	{
		bool HasLoaded
		{ get; }

		string LoadingText
		{ get; }
	}
}