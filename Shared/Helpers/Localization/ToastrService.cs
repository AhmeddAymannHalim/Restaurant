using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Shared.Constants;
using Shared.Enums.Others;

namespace Shared.Helpers.Localization
{
    public interface IToastrService
    {
        void Success(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }

    public class ToastrService : IToastrService
    {
        #region Fields
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
        #endregion

        #region Ctor
        public ToastrService(
            IHttpContextAccessor httpContextAccessor,
            ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
        }
        #endregion

        #region Utilities
        private void Notification(ToastrType type, string message)
        {
            var context = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(context);

            var toastrs = tempData.ContainsKey(ToastrDefaults.ToasterList)
                ? JsonConvert.DeserializeObject<IList<ToastrData>>(tempData[ToastrDefaults.ToasterList].ToString())
                : new List<ToastrData>();

            toastrs.Add(new ToastrData
            {
                Message = message,
                Type = type,
            });

            tempData[ToastrDefaults.ToasterList] = JsonConvert.SerializeObject(toastrs);
        }
        #endregion

        #region Methods
        public void Success(string message)
        {
            Notification(ToastrType.Success, message);
        }
        public void Info(string message)
        {
            Notification(ToastrType.Info, message);
        }
        public void Warning(string message)
        {
            Notification(ToastrType.Warning, message);
        }
        public void Error(string message)
        {
            Notification(ToastrType.Error, message);
        }
        #endregion
    }
}
