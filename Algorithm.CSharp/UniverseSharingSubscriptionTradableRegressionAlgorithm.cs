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
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This regression algorithm has two different Universe using the same SubscriptionDataConfig.
    /// Reproduces GH issue 3877: 1- universe 'TestUniverse' selects and deselects SPY. 2- UserDefinedUniverse
    /// reselects SPY, which should be marked as tradable.
    /// </summary>
    /// <meta name="tag" content="regression test" />
    public class UniverseSharingSubscriptionTradableRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Symbol _spy;
        private int _reselectedSpy = -1;
        private DateTime lastDataTime = DateTime.MinValue;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2013, 10, 01);
            SetEndDate(2013, 10, 30);
            AddEquity("AAPL", Resolution.Daily);

            UniverseSettings.Resolution = Resolution.Daily;
            AddUniverse(SecurityType.Equity,
                "TestUniverse",
                Resolution.Daily,
                Market.USA,
                UniverseSettings,
                time => time.Day == 1 ? new[] {"SPY"} : Enumerable.Empty<string>());
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (lastDataTime == data.Time)
            {
                throw new Exception("Duplicate time for current data and last data slice");
            }

            lastDataTime = data.Time;

            if (_reselectedSpy == 0)
            {
                if (!Securities[_spy].IsTradable)
                {
                    throw new Exception($"{_spy} should be tradable");
                }

                if (!Portfolio.Invested)
                {
                    SetHoldings(_spy, 1);
                }
            }

            if (_reselectedSpy == 1)
            {
                // SPY should be re added in the next loop
                _reselectedSpy = 0;
            }
        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            if (changes.RemovedSecurities.Any())
            {
                // OnSecuritiesChanged is called before OnData, so SPY will still not be
                // present
                _reselectedSpy = 1;
                _spy = AddEquity("SPY", Resolution.Daily).Symbol;

                if (!Securities[_spy].IsTradable)
                {
                    throw new Exception($"{_spy} should be tradable");
                }
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
        public long DataPoints => 229;

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
            {"Compounding Annual Return", "69.935%"},
            {"Drawdown", "2.000%"},
            {"Expectancy", "0"},
            {"Net Profit", "4.455%"},
            {"Sharpe Ratio", "4.41"},
            {"Probabilistic Sharpe Ratio", "82.955%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0.054"},
            {"Beta", "0.976"},
            {"Annual Standard Deviation", "0.106"},
            {"Annual Variance", "0.011"},
            {"Information Ratio", "5.672"},
            {"Tracking Error", "0.008"},
            {"Treynor Ratio", "0.48"},
            {"Total Fees", "$3.41"},
            {"Estimated Strategy Capacity", "$780000000.00"},
            {"Lowest Capacity Asset", "SPY R735QTJ8XC9X"},
            {"Return Over Maximum Drawdown", "34.803"},
            {"Portfolio Turnover", "0.038"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"OrderListHash", "f24baeb2d21ac305bc86e4ba4e9d4ece"}
        };
    }
}
