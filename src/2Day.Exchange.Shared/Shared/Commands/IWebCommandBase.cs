using System.Threading.Tasks;

namespace Chartreuse.Today.Exchange.Shared.Commands
{
    public interface IWebCommandBase<TResponseParser> where TResponseParser : IResponseParser
    {
        Task<ResponseResult<TResponseParser>> Execute();
    }
}