namespace TowerDefense.Shared
{
	/// <summary>
	/// Implement on any component that lives in an ObjectPool.
	/// Called by the pool manager on get and release.
	/// </summary>
	public interface IPoolable
	{
		void OnGetFromPool();
		void OnReturnToPool();
	}
}