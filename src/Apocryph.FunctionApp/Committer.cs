using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apocryph.FunctionApp.Model;
using Microsoft.Azure.WebJobs;
using Perper.WebJobs.Extensions.Config;
using Perper.WebJobs.Extensions.Model;

namespace Apocryph.FunctionApp
{
    public static class Committer
    {
        private class State
        {
            public Dictionary<Hash, Dictionary<ValidatorKey, ValidatorSignature>> Commits { get; set; } = new Dictionary<Hash, Dictionary<ValidatorKey, ValidatorSignature>>();
        }

        [FunctionName(nameof(Committer))]
        public static async Task Run([PerperStreamTrigger] PerperStreamContext context,
            [Perper("validatorSet")] ValidatorSet validatorSet,
            [PerperStream("commitsStream")] IAsyncEnumerable<ISigned<Commit>> commitsStream,
            [PerperStream("outputStream")] IAsyncCollector<Hash> outputStream)
        {
            var state = await context.FetchStateAsync<State>() ?? new State();

            await commitsStream.ForEachAsync(async commit =>
            {
                if (!state.Commits.ContainsKey(commit.Value.For))
                {
                    state.Commits[vote.Value.For] = new Dictionary<ValidatorKey, ValidatorSignature>();
                }
                state.Commits[commit.Value.For].Add(commit.Signer, commit.Signature);
                await context.UpdateStateAsync(state);

                var committed = state.Commits[commit.Value.For].Keys
                    .Select(signer => validatorSet.Weights[signer]).Sum();
                if (3 * committed > 2 * validatorSet.Total)
                {
                    await outputStream.AddAsync(commit.Value.For);
                }
            }, CancellationToken.None);
        }
    }
}