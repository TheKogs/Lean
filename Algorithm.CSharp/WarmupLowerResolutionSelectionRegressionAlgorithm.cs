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
using QuantConnect.Interfaces;
using System.Collections.Generic;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression algorithm asserting coarse universe selection behaves correctly during warmup when <see cref="IAlgorithmSettings.WarmupResolution"/> is set
    /// </summary>
    public class WarmupLowerResolutionSelectionRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Symbol _spy = QuantConnect.Symbol.Create("SPY", SecurityType.Equity, Market.USA);
        private Queue<DateTime> _selection = new Queue<DateTime>(new[]
        {
            new DateTime(2014, 03, 24),

            new DateTime(2014, 03, 25),
            new DateTime(2014, 03, 26),
            new DateTime(2014, 03, 27),
            new DateTime(2014, 03, 28),
            new DateTime(2014, 03, 29),

            new DateTime(2014, 04, 01),
            new DateTime(2014, 04, 02),
            new DateTime(2014, 04, 03),
            new DateTime(2014, 04, 04),
            new DateTime(2014, 04, 05),
        });

        public override void Initialize()
        {
            UniverseSettings.Resolution = Resolution.Hour;

            SetStartDate(2014, 03, 26);
            SetEndDate(2014, 04, 07);

            AddUniverse(CoarseSelectionFunction);
            SetWarmup(2, Resolution.Daily);
        }

        // sort the data by daily dollar volume and take the top 'NumberOfSymbols'
        private IEnumerable<Symbol> CoarseSelectionFunction(IEnumerable<CoarseFundamental> coarse)
        {
            var expected = _selection.Dequeue();
            if (expected != Time && !LiveMode)
            {
                throw new Exception($"Unexpected selection time: {Time}. Expected {expected}");
            }

            Debug($"Coarse selection happening at {Time} {IsWarmingUp}");
            return new[] { _spy };
        }

        public override void OnData(Slice slice)
        {
            var expectedDataSpan = QuantConnect.Time.OneHour;
            if (Time <= StartDate)
            {
                expectedDataSpan = QuantConnect.Time.OneDay;
            }

            foreach (var data in slice.Values)
            {
                var dataSpan = data.EndTime - data.Time;
                if (dataSpan != expectedDataSpan)
                {
                    throw new Exception($"Unexpected bar span! {data}: {dataSpan} Expected {expectedDataSpan}");
                }
            }

            Debug($"OnData({UtcTime:o}): {IsWarmingUp}. {string.Join(", ", slice.Values.OrderBy(x => x.Symbol))}");

            if (!Portfolio.Invested && !IsWarmingUp)
            {
                SetHoldings(_spy, 1m);
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
        public long DataPoints => 78099;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "1"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "-33.204%"},
            {"Drawdown", "2.600%"},
            {"Expectancy", "0"},
            {"Net Profit", "-1.427%"},
            {"Sharpe Ratio", "-0.671"},
            {"Probabilistic Sharpe Ratio", "35.939%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0"},
            {"Beta", "1.001"},
            {"Annual Standard Deviation", "0.097"},
            {"Annual Variance", "0.009"},
            {"Information Ratio", "-0.538"},
            {"Tracking Error", "0"},
            {"Treynor Ratio", "-0.065"},
            {"Total Fees", "$3.07"},
            {"Estimated Strategy Capacity", "$120000000.00"},
            {"Lowest Capacity Asset", "SPY R735QTJ8XC9X"},
            {"Return Over Maximum Drawdown", "-12.318"},
            {"Portfolio Turnover", "0.077"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"OrderListHash", "58e44f28fddb48a935ab94e4b19a1727"}
        };
    }
}
