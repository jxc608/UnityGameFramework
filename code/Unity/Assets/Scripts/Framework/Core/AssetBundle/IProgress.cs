
public delegate string GetProgressText(long inSize, long outSize);
public delegate string GetProgressText2(long inSize, long outSize, string extra);

public interface IProgress
{
	void ResetProgress();

	void SetTextFunc(GetProgressText func);

	void SetTextFunc2(GetProgressText2 func);

	void SetProgress(long inSize, long outSize, string extra = "");
};