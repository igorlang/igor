using Igor.Erlang.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.Json
{
    public static class ErlJsonRpc
    {
        public static string FormatRequest(string method, IEnumerable<FunctionArgument> arguments, string id = null)
        {
            var r = new Renderer();
            if (arguments.Any())
            {
                r += "#{";
                r++;
                r += $@"<<""method"">> => <<""{method}"">>,";
                if (id != null)
                    r += $@"<<""id"">> => {id},";
                r += @"<<""params"">> => [";
                r++;
                r.Blocks(arguments.Select(arg => arg.erlJsonTag.PackJson(arg.erlVarName)), delimiter: ",");
                r--;
                r += "]";
                r--;
                r += "}";
            }
            else if (id != null)
            {
                r += $@"#{{<<""method"">> => <<""{method}"">>, <<""id"">> => {id}}}";
            }
            else
            {
                r += $@"#{{<<""method"">> => <<""{method}"">>}}";
            }
            return r.Build().TrimEnd();
        }

        public static string FormatResult(string method, IEnumerable<FunctionArgument> arguments, string id = null)
        {
            var r = new Renderer();
            if (arguments.Any())
            {
                r += "#{";
                r++;
                r += $@"<<""method"">> => <<""{method}"">>,";
                if (id != null)
                    r += $@"<<""id"">> => {id},";
                r += @"<<""result"">> => [";
                r++;
                r.Blocks(arguments.Select(arg => arg.erlJsonTag.PackJson(arg.erlVarName)), delimiter: ",");
                r--;
                r += "]";
                r--;
                r += "}";
            }
            else if (id != null)
            {
                r += $@"#{{<<""method"">> => <<""{method}"">>, <<""result"">> => [], <<""id"">> => {id}}}";
            }
            else
            {
                r += $@"#{{<<""method"">> => <<""{method}"">>, <<""result"">> => []}}";
            }
            return r.Build().TrimEnd();
        }

        public static string FormatFail(string method, int errorCode, string errorMessage, string errorData, string id)
        {
            var r = new Renderer();
            r += "#{";
            r++;
            r += $@"<<""method"">> => <<""{method}"">>,";
            if (id != null)
                r += $@"<<""id"">> => {id},";
            r += @"<<""error"">> => #{";
            r++;
            r += $@"<<""code"">> => {errorCode},";
            if (errorData != null)
            {
                r += $@"<<""message"">> => {errorMessage},";
                r += $@"<<""data"">> => {errorData}";
            }
            else
            {
                r += $@"<<""message"">> => {errorMessage}";
            }
            r--;
            r += "}";
            r--;
            r += "}";
            return r.Build().TrimEnd();
        }
    }
}
