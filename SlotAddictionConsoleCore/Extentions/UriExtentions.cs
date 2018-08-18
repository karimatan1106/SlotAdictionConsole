using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlotAddictionCore.Extentions
{
    public static class UriExtentions
    {
        /// <summary>
        /// URIのコレクション内で存在するURIだけを返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="httpClient"></param>
        public static async Task<IEnumerable<Uri>> GetExistUriAsync(this IEnumerable<Uri> self, HttpClient httpClient)
        {
            if (self == null)
            {
                throw new NullReferenceException($"nullに対して{nameof(GetExistUriAsync)}を呼び出すことはできません。");
            }

            //URIが存在するかどうかをチェック
            var requestCheckTasks = self.Select(httpClient.GetAsync);
            var requestCheckResponses = await Task.WhenAll(requestCheckTasks);

            //存在するURIを取得
            return requestCheckResponses
                .Where(x => x.StatusCode == HttpStatusCode.OK)
                .Select(x => x.RequestMessage.RequestUri);
        }
    }
}
