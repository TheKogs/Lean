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

using System.Linq;
using System.Collections.Generic;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Interfaces;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.Algorithm.CSharp
{
    public class BlackLittermanPortfolioOptimizationFrameworkAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private IEnumerable<Symbol> _symbols = (new string[] { "AIG", "BAC", "IBM", "SPY" }).Select(s => QuantConnect.Symbol.Create(s, SecurityType.Equity, Market.USA));

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            // Set requested data resolution
            UniverseSettings.Resolution = Resolution.Minute;

            // Order margin value has to have a minimum of 0.5% of Portfolio value, allows filtering out small trades and reduce fees.
            // Commented so regression algorithm is more sensitive
            //Settings.MinimumOrderMarginPortfolioPercentage = 0.005m;

            SetStartDate(2013, 10, 07);  //Set Start Date
            SetEndDate(2013, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash

            // Find more symbols here: http://quantconnect.com/data
            // Forex, CFD, Equities Resolutions: Tick, Second, Minute, Hour, Daily.
            // Futures Resolution: Tick, Second, Minute
            // Options Resolution: Minute Only.

            var optimizer = new UnconstrainedMeanVariancePortfolioOptimizer();

            // set algorithm framework models
            SetUniverseSelection(new CoarseFundamentalUniverseSelectionModel(CoarseSelector));
            SetAlpha(new HistoricalReturnsAlphaModel(resolution: Resolution.Daily));
            SetPortfolioConstruction(new BlackLittermanOptimizationPortfolioConstructionModel(optimizer: optimizer));
            SetExecution(new ImmediateExecutionModel());
            SetRiskManagement(new NullRiskManagementModel());
        }

        public IEnumerable<Symbol> CoarseSelector(IEnumerable<CoarseFundamental> coarse)
        {
            int last = Time.Day > 8 ? 3 : _symbols.Count();
            return _symbols.Take(last);
        }

        public bool CanRunLocally => true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 14082;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 256;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "20"},
            {"Average Win", "0%"},
            {"Average Loss", "-0.13%"},
            {"Compounding Annual Return", "62.435%"},
            {"Drawdown", "1.100%"},
            {"Expectancy", "-1"},
            {"Net Profit", "0.667%"},
            {"Sharpe Ratio", "3.993"},
            {"Probabilistic Sharpe Ratio", "58.777%"},
            {"Loss Rate", "100%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-0.598"},
            {"Beta", "0.569"},
            {"Annual Standard Deviation", "0.133"},
            {"Annual Variance", "0.018"},
            {"Information Ratio", "-13.973"},
            {"Tracking Error", "0.104"},
            {"Treynor Ratio", "0.932"},
            {"Total Fees", "$46.20"},
            {"Estimated Strategy Capacity", "$2300000.00"},
            {"Lowest Capacity Asset", "AIG R735QTJ8XC9X"},
            {"Return Over Maximum Drawdown", "65.642"},
            {"Portfolio Turnover", "0.645"},
            {"Total Insights Generated", "13"},
            {"Total Insights Closed", "10"},
            {"Total Insights Analysis Completed", "10"},
            {"Long Insight Count", "6"},
            {"Short Insight Count", "7"},
            {"Long/Short Ratio", "85.71%"},
            {"OrderListHash", "0945ff7a39bb8f8a07b3dcc817c070aa"}
        };
    }
}
