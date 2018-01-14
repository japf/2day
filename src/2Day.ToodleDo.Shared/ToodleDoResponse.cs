namespace Chartreuse.Today.ToodleDo
{
    public struct ToodleDoResponse<T>
    {
        public bool HasError
        {
            get { return !string.IsNullOrEmpty(this.Error); }
        }

        public string Error { get; private set; }
        
        public T Data { get; private set; }

        public ToodleDoResponse(T data) : this()
        {
            this.Data = data;
        }

        public ToodleDoResponse(bool e, string error) : this()
        {
            this.Error = error;
            this.Data = default(T);
        }
    }
}