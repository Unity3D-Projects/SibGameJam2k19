using SMGCore;

public sealed class KOZAPersistence : PersistentDataHolder {
	public bool   IsWin                = false;
	public bool   FastRestart          = false;
	public string LastLevelName        = null;
	public float  TotalDistance        = 0f;
	public bool   EndlessLevel         = false;
	public int    ConsecutiveFailCount = 0;
	public int    AdditionalScore      = 0;
}
