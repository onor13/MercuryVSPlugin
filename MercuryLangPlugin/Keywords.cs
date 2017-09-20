using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryLangPlugin
{
    public static class Keywords
    {
        public enum KeywordType
        {
            None,
            BoolValue,
            Declaration,
            Foreign,
            ForeignMode,
            Implementation,
            Import,
            Keyword,
            Pragma,
            Logical,
            Operator,
            Purity
        }

        public static bool IsMercuryKeyword(string token)
        {
            return MercuryKeywordsDic.ContainsKey(token);
        }

        public static KeywordType GetType(string keyword)
        {
            KeywordType type;
            MercuryKeywordsDic.TryGetValue(keyword, out type);
            return type;
        }

        private static IReadOnlyCollection<string> mercuryKeywords;
        public static IReadOnlyCollection<string> GetMercuryKeywords()
        {
            if(mercuryKeywords == null)
            {
                mercuryKeywords = new ReadOnlyCollection<string>(new List<string>(MercuryKeywordsDic.Keys));
            }
            return mercuryKeywords;
        }
      
        private static IReadOnlyDictionary<string, KeywordType> MercuryKeywordsDic = new Dictionary<string, KeywordType>()
        {
            //A
            {"affects_liveness", KeywordType.ForeignMode},
              {"attach_to_io_state", KeywordType.ForeignMode},
             { "all", KeywordType.Logical },
            { "any_func", KeywordType.None },
            { "any_pred", KeywordType.None },
            { "arbitrary", KeywordType.None },
            { "atomic", KeywordType.None },

            //C
             {"can_pass_as_mercury_type", KeywordType.ForeignMode},
            { "catch", KeywordType.None },
            { "catch_any", KeywordType.None },
            { "cc_multi", KeywordType.None },
            { "cc_nondet", KeywordType.None },
            { "check_termination", KeywordType.Pragma },
             { "consider_used", KeywordType.Pragma },

            //D
            { "det", KeywordType.None },
            { "div", KeywordType.Operator },
             {"does_not_affect_liveness", KeywordType.ForeignMode},
              {"doesnt_affect_liveness", KeywordType.ForeignMode},
            { "does_not_terminate", KeywordType.Pragma },

            //E
            { "else", KeywordType.Logical},
            { "end_module", KeywordType.None },
            { "erroneous", KeywordType.None },
            { "external", KeywordType.None },
            { "external_pred", KeywordType.None },
            { "external_func", KeywordType.None },

            //F
            { "fact_table", KeywordType.Pragma },
            { "fail", KeywordType.Logical },
            { "failure", KeywordType.None },
            { "false", KeywordType.Logical },
            { "finalize", KeywordType.None },
            { "foreign_code", KeywordType.Foreign },
            { "foreign_decl", KeywordType.Foreign },
            { "foreign_enum", KeywordType.Foreign },
            { "foreign_export", KeywordType.Foreign },
             { "foreign_export_enum", KeywordType.Foreign },
             { "foreign_import_module", KeywordType.Foreign },
            { "foreign_proc", KeywordType.Foreign },
            { "foreign_type", KeywordType.Foreign },
            { "func", KeywordType.Declaration },

            //I
            { "if", KeywordType.Logical },
            { "implementation", KeywordType.Implementation },
            { "import_module", KeywordType.Import },
            { "impure", KeywordType.Purity },
            { "impure_true", KeywordType.Logical},
            { "include_module", KeywordType.None },
            { "initialise", KeywordType.None },
            { "initialize", KeywordType.None },
            { "inline", KeywordType.Pragma },
            { "inst", KeywordType.Declaration },
            { "instance", KeywordType.Declaration },
            { "interface", KeywordType.None },
            { "is", KeywordType.None },

            //L
            { "local", KeywordType.None },
            { "loop_check", KeywordType.Pragma },

            //M
            { "maybe_thread_safe", KeywordType.None },
             { "may_call_mercury", KeywordType.ForeignMode },
             { "may_duplicate", KeywordType.ForeignMode },
            { "may_not_duplicate", KeywordType.ForeignMode },
            { "may_modify_trail", KeywordType.ForeignMode },
            { "memo", KeywordType.Pragma },
            { "minimal_model", KeywordType.Pragma },
            { "mod",KeywordType.Operator },
            { "mode", KeywordType.None },
            { "module", KeywordType.Declaration },
            { "multi", KeywordType.None },
            { "mutable", KeywordType.Declaration },

            //N
            { "no", KeywordType.BoolValue },
            { "no_inline", KeywordType.Pragma },
            { "nondet", KeywordType.None },
             { "no_sharing", KeywordType.ForeignMode },
            { "not", KeywordType.Logical },
             { "not_thread_safe", KeywordType.ForeignMode },

            //O
            { "obsolete", KeywordType.Pragma },
            { "or_else", KeywordType.None },

            //P
            { "pragma", KeywordType.None },
            { "pred", KeywordType.Declaration },
            { "promise", KeywordType.None },
            { "promise_equivalent_clauses", KeywordType.Pragma },
            { "promise_equivalent_solutions", KeywordType.None },
            { "promise_equivalent_solution_sets", KeywordType.None },
            { "promise_impure", KeywordType.Purity },
            { "promise_pure", KeywordType.Purity },
            { "promise_semipure", KeywordType.Purity },

            //R
            { "require_cc_multi", KeywordType.None },
            { "require_cc_nondet", KeywordType.None },
            { "require_complete_switch", KeywordType.None },
            { "require_det", KeywordType.None },
            { "require_erroneous", KeywordType.None },
            { "require_failure", KeywordType.None },
            { "require_multi", KeywordType.None },
            { "require_nondet", KeywordType.None },
            { "require_semidet", KeywordType.None },
            { "require_switch_arms_cc_multi", KeywordType.None },
            { "require_switch_arms_cc_nondet", KeywordType.None },
            { "require_switch_arms_det", KeywordType.None },
            { "require_switch_arms_erroneous", KeywordType.None },
            { "require_switch_arms_failure", KeywordType.None },
            { "require_switch_arms_multi", KeywordType.None },
            { "require_switch_arms_nondet", KeywordType.None },
            { "require_switch_arms_semidet", KeywordType.None },
            {"rem",KeywordType.Operator },

            //S
            { "semidet", KeywordType.None },
            { "semidet_fail", KeywordType.Logical },
            { "semidet_false", KeywordType.Logical },
            { "semidet_succeed", KeywordType.Logical },
            { "semidet_true", KeywordType.Logical },
            { "semipure", KeywordType.Purity },
            { "sharing", KeywordType.ForeignMode },
            { "solver", KeywordType.None },
            { "source_file", KeywordType.Pragma },
            { "some", KeywordType.Logical },           
            { "stable", KeywordType.ForeignMode},

            //T
            { "tabled_for_io", KeywordType.ForeignMode },
            { "terminates", KeywordType.Pragma },
            { "then", KeywordType.Logical},
            { "thread_safe", KeywordType.None },
            { "trace", KeywordType.None },
            { "trailed", KeywordType.None },
            { "true", KeywordType.Logical },
            { "try", KeywordType.None },
            { "type", KeywordType.Declaration },
            { "typeclass", KeywordType.Declaration },
            { "type_spec", KeywordType.Pragma },

            //U
             { "unknown_sharing", KeywordType.ForeignMode },
            {"untrailed", KeywordType.None },
            { "use_module", KeywordType.None },

            //W
             { "will_not_call_mercury", KeywordType.ForeignMode },
              { "will_not_modify_trail", KeywordType.ForeignMode },
              { "will_not_throw_exception", KeywordType.ForeignMode },
            { "where", KeywordType.None },

            //Y
            { "yes", KeywordType.BoolValue },

        };
    }

}
