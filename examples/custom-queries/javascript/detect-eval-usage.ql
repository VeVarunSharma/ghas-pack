/**
 * @name Use of eval
 * @description Using eval() can lead to code injection vulnerabilities.
 *              Attackers may exploit eval to execute arbitrary code if
 *              user-controlled input reaches the eval call.
 * @kind problem
 * @problem.severity warning
 * @id ghas-pack/js/eval-usage
 * @tags security
 *       correctness
 */

// Import the CodeQL JavaScript analysis library.
// This provides classes like CallExpr for modeling JS syntax.
import javascript

// from: Declare a variable `call` of type CallExpr — represents any
//       function call expression in the JavaScript source code.
// where: Filter to only calls where the callee name is "eval".
// select: Report each matching call with an explanatory message.
from CallExpr call
where call.getCalleeName() = "eval"
select call, "Avoid using eval(). Consider safer alternatives like JSON.parse() for data parsing."
