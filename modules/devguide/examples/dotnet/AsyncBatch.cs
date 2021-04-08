using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.KeyValue;

namespace Couchbase.Net.DevGuide
{
    class AsyncBatch : ConnectionBase
    {
        private new static async Task Main(string[] args)
        {
            await new AsyncBatch().ExecuteAsync().ConfigureAwait(false);
            Console.Read();
        }

        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var ids = new List<string> { "doc1", "doc2", "doc4" };

            // ReSharper disable once IdentifierTypo
            var upserts = new List<Task<IMutationResult>>();
            ids.ForEach(x => upserts.Add(Bucket.DefaultCollection().UpsertAsync(x, x)));
            await Task.WhenAll(upserts).ConfigureAwait(false);

            var gets = new List<Task<IGetResult>>();
            ids.ForEach(x => gets.Add(Bucket.DefaultCollection().GetAsync(x)));

            var results = await Task.WhenAll(gets).ConfigureAwait(false);
            results.ToList().ForEach(doc => Console.WriteLine("Removed " + doc.ContentAs<dynamic>()));
        }
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2020 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion
