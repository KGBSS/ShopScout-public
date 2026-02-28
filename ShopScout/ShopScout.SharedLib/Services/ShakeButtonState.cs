namespace ShopScout.Client
{
    public class ShakeButtonState
    {
        public event Action<object>? OnReject;

        public void Reject(object t)
        {
            OnReject?.Invoke(t);
        }
    }
}
