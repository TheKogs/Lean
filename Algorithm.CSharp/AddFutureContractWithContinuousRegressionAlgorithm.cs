/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Orders;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using System.Collections.Generic;
using QuantConnect.Securities.Future;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Continuous Futures Regression algorithm. Asserting and showcasing the behavior of adding a continuous future
    /// and a future contract at the same time
    /// </summary>
    public class AddFutureContractWithContinuousRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Symbol _currentMappedSymbol;
        private Future _continuousContract;
        private Future _futureContract;
        private bool _ended;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2013, 10, 6);
            SetEndDate(2013, 10, 10);

            _continuousContract = AddFuture(Futures.Indices.SP500EMini,
                dataNormalizationMode: DataNormalizationMode.BackwardsRatio,
                dataMappingMode: DataMappingMode.LastTradingDay,
                contractDepthOffset: 0
            );

            _futureContract = AddFutureContract(FutureChainProvider.GetFutureContractList(_continuousContract.Symbol, Time).First());
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (_ended)
            {
                throw new Exception($"Algorithm should of ended!");
            }
            if (data.Keys.Count > 2)
            {
                throw new Exception($"Getting data for more than 2 symbols! {string.Join(",", data.Keys.Select(symbol => symbol))}");
            }
            if (UniverseManager.Count != 3)
            {
                throw new Exception($"Expecting 3 universes (chain, continuous and user defined) but have {UniverseManager.Count}");
            }

            if (!Portfolio.Invested)
            {
                Buy(_futureContract.Symbol, 1);
                Buy(_continuousContract.Mapped, 1);

                RemoveSecurity(_futureContract.Symbol);
                RemoveSecurity(_continuousContract.Symbol);

                _ended = true;
            }
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status == OrderStatus.Filled)
            {
                Log($"{orderEvent}");
            }
        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            Debug($"{Time}-{changes}");

            if (changes.AddedSecurities.Any(security => security.Symbol != _continuousContract.Symbol && security.Symbol != _futureContract.Symbol)
                || changes.RemovedSecurities.Any(security => security.Symbol != _continuousContract.Symbol && security.Symbol != _futureContract.Symbol))
            {
                throw new Exception($"We got an unexpected security changes {changes}");
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 63;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "3"},
            {"Average Win", "0%"},
            {"Average Loss", "-0.03%"},
            {"Compounding Annual Return", "-2.594%"},
            {"Drawdown", "0.000%"},
            {"Expectancy", "-1"},
            {"Net Profit", "-0.034%"},
            {"Sharpe Ratio", "-7.854"},
            {"Probabilistic Sharpe Ratio", "1.216%"},
            {"Loss Rate", "100%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-0.022"},
            {"Beta", "0.004"},
            {"Annual Standard Deviation", "0.003"},
            {"Annual Variance", "0"},
            {"Information Ratio", "-0.768"},
            {"Tracking Error", "0.241"},
            {"Treynor Ratio", "-4.689"},
            {"Total Fees", "$8.60"},
            {"Estimated Strategy Capacity", "$5500000.00"},
            {"Lowest Capacity Asset", "ES VMKLFZIH2MTD"},
            {"Return Over Maximum Drawdown", "-81.518"},
            {"Portfolio Turnover", "0.834"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"OrderListHash", "802a335b5c355e83b8cd2174f053c1b9"}
        };
    }
}
