using System;

namespace Chartreuse.Today.Exchange.Shared.Commands
{
    public class ResponseResult<T> where T : IResponseParser
    {
        public T Data { get; private set; }

        public Exception Error { get; private set; }

        public string Uri { get; private set; }

        internal static ResponseResult<T> Create(T data, string uri)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            return new ResponseResult<T>() { Error = null, Data = data, Uri = uri };
        }

        internal static ResponseResult<T> Create(Exception exception, string uri)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            return new ResponseResult<T>() { Error = exception, Data = default(T), Uri = uri };
        }
    }
}
