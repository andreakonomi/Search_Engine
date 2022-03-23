using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Library.Internal
{
    internal  class QueryBuilder
    {
        /// <summary>
        /// Parses the initial query and forms the 
        /// </summary>
        /// <param name="query">The query to be parsed for the search</param>
        /// <param name="dynPars">The dynamic parameters for the dapper execution</param>
        /// <returns>The formed sql query to be executed</returns>
        public string CreateQueryForData(string query, ref DynamicParameters dynPars)
        {
            //a
            //a & b
            //a & (b | c)
            string finalQuery = "";

            query = query.Replace("(", null).Replace(")", null);
            var splittedArgs = query.Split(" ", StringSplitOptions.TrimEntries);
            int count = splittedArgs.Length;

            finalQuery = CreateSingleArgQuery(ref dynPars, splittedArgs[0]);

            if (count == 3)
            {
                string logicOperator = splittedArgs[1];
                string nextValue = splittedArgs[2];
                return CreateQueryForDoubleParameters(ref dynPars, finalQuery, nextValue, logicOperator);
            }

            if (count == 5)
            {
                return HandleMultipleParameters(query, ref dynPars);
            }

            if (count > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(query), "Query is too long to process.");
            }

            return finalQuery;
        }

        /// <summary>
        /// Creates the sql query and populates the args for the single argument case.
        /// </summary>
        /// <param name="dynPars">Reference that holds the sql parameters</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns></returns>
        private string CreateSingleArgQuery(ref DynamicParameters dynPars, string value)
        {
            dynPars.Add("@par1", value);
            return $"SELECT DISTINCT DocumentId FROM Tokens WHERE Content = @par1";
        }

        /// <summary>
        /// Creates the sql query and populates the args for the double argument case.
        /// </summary>
        /// <param name="dynPars">Reference that holds the sql parameters</param>
        /// <returns></returns>
        private string CreateQueryForDoubleParameters(ref DynamicParameters dynPars, string query, string secondArg, string logicOperator)
        {
            dynPars.Add("@par2", secondArg);

            if (logicOperator == "|")
            {
                query = AddOrClauseForNextArg(query);
            }
            else if (logicOperator == "&")
            {
                query = AddAndClauseForNextArg(query);
            }

            return query;
        }

        /// <summary>
        /// Constructs and returns the formated sql query for triple arguments provided expression.
        /// </summary>
        /// <param name="initialQuery">The initialQuery passed</param>
        /// <param name="dynPars">Reference that holds the sql parameters.</param>
        /// <returns>The constructed sql query for the search</returns>
        private string HandleMultipleParameters(string initialQuery, ref DynamicParameters dynPars)
        {
            bool onlyAnds = !initialQuery.Contains('|');
            bool onlyOrs = !initialQuery.Contains('&');
            string finalQuery = "";

            if (onlyAnds || onlyOrs)
            {
                initialQuery = RemoveOperatorsSymbolsfromQuery(initialQuery);

                var splittedArray = initialQuery.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                string par1 = splittedArray[0];
                string par2 = splittedArray[1];
                string par3 = splittedArray[2];

                if (onlyAnds)
                {
                    finalQuery = GiveOnlyDoubleAndsQuery(ref dynPars, par1, par2, par3);
                }

                if (onlyOrs)
                {
                    finalQuery = GiveOnlyDoubleOrsQuery(ref dynPars, par1, par2, par3);
                }
            }
            else
            {
                finalQuery = CalculateMixedOperatorsQuery(ref dynPars, initialQuery);
            }

            return finalQuery;
        }

        /// <summary>
        /// Constructs the sql query for triple arguments passed in and populates the 
        /// sql paramters objects with their values.
        /// </summary>
        /// <param name="dynPars">Reference that holds the sql parameters and their values.</param>
        /// <param name="initialQuery">The initial query passed in</param>
        /// <returns></returns>
        private string CalculateMixedOperatorsQuery(ref DynamicParameters dynPars, string initialQuery)
        {
            string query = "";
            string par1 = null, par2 = null, par3 = null, par4 = null;
            string a, b, c;

            query = @"
                SELECT DocumentId FROM Tokens
                WHERE Content IN (@par1, @par2)
                INTERSECT
                SELECT DocumentId FROM Tokens
                WHERE Content IN (@par3, @par4)
                ";

            int indexOfAndOperator = initialQuery.IndexOf('&');
            int indexOfOrOperator = initialQuery.IndexOf('|');
            int indexOfOpeningBrace = initialQuery.IndexOf('(');
            int indexOfClosingBrace = initialQuery.IndexOf(')');

            initialQuery = RemoveOperatorsSymbolsfromQuery(initialQuery);
            var arrayArguments = initialQuery.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            a = arrayArguments[0];
            b = arrayArguments[1];
            c = arrayArguments[2];

            // mode (a & b) | c ; mode a | (b & c)
            if (indexOfAndOperator > indexOfOpeningBrace && indexOfAndOperator < indexOfClosingBrace)
            {
                if (indexOfClosingBrace < indexOfOrOperator)
                {
                    // mode (a & b) | c = (a | c) & (b | c)
                    par1 = a;
                    par2 = c;
                    par3 = b;
                    par4 = c;
                }
                else
                {
                    // mode a | (b & c) = (a | b) & (a | c)
                    par1 = a;
                    par2 = b;
                    par3 = a;
                    par4 = c;
                }
            }
            else
            {
                // mode (a | b) & c ; // mode a & (b | c)
                if (indexOfClosingBrace < indexOfAndOperator)
                {
                    // mode (a | b) & (c | d) ; d is null
                    par1 = a;
                    par2 = b;
                    par3 = c;
                }
                else
                {
                    // mode (a | b) & (b | c) ; b is null
                    par1 = a;
                    par3 = b;
                    par4 = c;
                }
            }

            dynPars.Add("@par1", par1);
            dynPars.Add("@par2", par2);
            dynPars.Add("@par3", par3);
            dynPars.Add("@par4", par4);

            return query;
        }

        /// <summary>
        /// Removes the (, ), &, | and empty characters from the query.
        /// </summary>
        private string RemoveOperatorsSymbolsfromQuery(string query)
        {
            return query.Replace("&", null).Replace("|", null).Replace("(", null).Replace(")", null);
        }

        /// <summary>
        /// Constructs the query for a & b & c case
        /// </summary>
        /// <param name="dynPars">Reference that holds the sql parameters and their values.</param>
        /// <param name="par1">First parameter given</param>
        /// <param name="par2">Second parameter given</param>
        /// <param name="par3">Third parameter given</param>
        /// <returns>The constructed sql query for this case</returns>
        private string GiveOnlyDoubleAndsQuery(ref DynamicParameters dynPars, string par1, string par2, string par3)
        {
            dynPars.Add("@par1", par1);
            dynPars.Add("@par2", par2);
            dynPars.Add("@par3", par3);

            return
                $@"
            SELECT DocumentId FROM Tokens WHERE Content = @par1
            INTERSECT
            SELECT DocumentId FROM Tokens WHERE Content = @par2
            INTERSECT
            SELECT DocumentId FROM Tokens WHERE Content = @par3
                ";
        }

        /// <summary>
        /// Constructs the query for the a | b | c case
        /// </summary>
        /// <param name="dynPars">Reference that holds the sql parameters and their values.</param>
        /// <returns></returns>
        private string GiveOnlyDoubleOrsQuery(ref DynamicParameters dynPars, string par1, string par2, string par3)
        {
            dynPars.Add("@par1", par1);
            dynPars.Add("@par2", par2);
            dynPars.Add("@par3", par3);

            return
                $@"
            SELECT DISTINCT DocumentId from Tokens 
            WHERE Content IN (@par1, @par2, @par3)";
        }

        /// <summary>
        /// Appends the second part of the sql query for the and case filter
        /// </summary>
        /// <param name="initialQuery">Initial query being built</param>
        /// <returns>Formatted query</returns>
        private string AddAndClauseForNextArg(string initialQuery)
        {
            return $"{initialQuery} \nINTERSECT\nSELECT DISTINCT DocumentId FROM Tokens WHERE Content = @par2";
        }

        /// <summary>
        /// Appends the second part of the sql query for the or case filter
        /// </summary>
        /// <param name="initialQuery">Initial query being built</param>
        /// <returns>Formatted query</returns>

        private string AddOrClauseForNextArg(string initialQuery)
        {
            return $"{initialQuery} OR Content = @par2";
        }

    }
}
