/**
 * @name Use of exec or eval
 * @description Using exec() or eval() can lead to code injection vulnerabilities.
 *              Attackers may exploit these functions to execute arbitrary code if
 *              user-controlled input is passed to them.
 * @kind problem
 * @problem.severity warning
 * @id ghas-pack/py/exec-usage
 * @tags security
 */

// Import the CodeQL Python analysis library.
// This provides classes like Call and Name for modeling Python syntax.
import python

// from: Declare `call` (a function call node) and `func` (the name expression
//       being called). These represent Python call expressions like exec(...).
// where: Match calls where the function is a bare name that is either
//        "exec" or "eval" — the most common dangerous built-in functions.
// select: Report the call with a message that includes the function name.
from Call call, Name func
where
  call.getFunc() = func and
  (func.getId() = "exec" or func.getId() = "eval")
select call, "Avoid using " + func.getId() + "(). Consider safer alternatives."
