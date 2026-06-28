package tests
{
   public class TestNamedAnonFunctions
   {
      public function run() : *
      {
         var test:* = function testFunc(param1:*, param2:int, param3:Array):Boolean
			{
				return (param1 as TestClass2).attrib1 == 5;
			};
        f(5, function g():int {
            return 42;            
        });
      }
      
      private function f(a:int,b:Function) 
      {
      }
   }
}

class TestClass2
{
	public var attrib1:int;
}
	
