package tests
{
   public class TestNamedAnonFunctions
   {
      
      method
         name ""
         returns null
         
         body
            maxstack 1
            localcount 1
            initscopedepth 3
            maxscopedepth 4
            
            code
               getlocal0
               pushscope
               returnvoid
            end ; code
         end ; body
      end ; method
      
      public function TestNamedAnonFunctions()
      {
         method
            name "tests:TestNamedAnonFunctions/TestNamedAnonFunctions"
            returns null
            
            body
               maxstack 1
               localcount 1
               initscopedepth 4
               maxscopedepth 5
               
               code
                  getlocal0
                  pushscope
                  getlocal0
                  constructsuper 0
                  returnvoid
               end ; code
            end ; body
         end ; method
      }
      
      public function run() : *
      {
         trait method QName(PackageNamespace(""),"run")
            dispid 0
            method
               name "tests:TestNamedAnonFunctions/run"
               flag NEED_ACTIVATION
               returns null
               
               body
                  maxstack 5
                  localcount 2
                  initscopedepth 5
                  maxscopedepth 8
                  trait slot QName(PackageInternalNs("tests"),"test")
                     slotid 1
                     type null
                  end ; trait
                  
                  code
                     getlocal0
                     pushscope
                     debug 1, "+$activation", 0, 0
                     newactivation
                     dup
                     setlocal1
                     pushscope
                     getscopeobject 1
                     newobject 0
                     pushwith
                     newfunction 3
                     dup
                     getscopeobject 2
                     swap
                     setproperty QName(PackageNamespace("tests"),"testFunc")
                     popscope
                     coerce_a
                     setslot 1
                     getlocal0
                     pushbyte 5
                     newobject 0
                     pushwith
                     newfunction 4
                     dup
                     getscopeobject 2
                     swap
                     setproperty QName(PackageNamespace("tests"),"g")
                     popscope
                     callpropvoid QName(PrivateNamespace("tests:TestNamedAnonFunctions"),"f"), 2
                     returnvoid
                  end ; code
               end ; body
            end ; method
         }
         
         private function f(a:int, b:Function) : *
         {
            trait method QName(PrivateNamespace("tests:TestNamedAnonFunctions"),"f")
               dispid 0
               method
                  name "tests:TestNamedAnonFunctions/private/f"
                  param QName(PackageNamespace(""),"int")
                  param QName(PackageNamespace(""),"Function")
                  returns null
                  
                  body
                     maxstack 1
                     localcount 3
                     initscopedepth 4
                     maxscopedepth 5
                     
                     code
                        getlocal0
                        pushscope
                        debug 1, "a", 0, 0
                        debug 1, "b", 1, 0
                        returnvoid
                     end ; code
                  end ; body
               end ; method
            }
         }
      }
      
      class TestClass2
      {
         
         method
            name ""
            returns null
            
            body
               maxstack 1
               localcount 1
               initscopedepth 3
               maxscopedepth 4
               
               code
                  getlocal0
                  pushscope
                  returnvoid
               end ; code
            end ; body
         end ; method
         
         public var attrib1:int;
         
         public function TestClass2()
         {
            method
               name "TestNamedAnonFunctions.as$0:TestClass2/TestClass2"
               returns null
               
               body
                  maxstack 1
                  localcount 1
                  initscopedepth 4
                  maxscopedepth 5
                  
                  code
                     getlocal0
                     pushscope
                     getlocal0
                     constructsuper 0
                     returnvoid
                  end ; code
               end ; body
            end ; method
         }
      }
      
      method
         name ""
         returns null
         
         body
            maxstack 2
            localcount 1
            initscopedepth 1
            maxscopedepth 3
            
            code
               getlocal0
               pushscope
               findpropstrict Multiname("TestNamedAnonFunctions",[PackageNamespace("tests")])
               getlex QName(PackageNamespace(""),"Object")
               pushscope
               getlex QName(PackageNamespace(""),"Object")
               newclass 0
               popscope
               initproperty QName(PackageNamespace("tests"),"TestNamedAnonFunctions")
               findpropstrict Multiname("TestClass2",[PrivateNamespace("TestNamedAnonFunctions.as$0")])
               getlex QName(PackageNamespace(""),"Object")
               pushscope
               getlex QName(PackageNamespace(""),"Object")
               newclass 1
               popscope
               initproperty QName(PrivateNamespace("TestNamedAnonFunctions.as$0"),"TestClass2")
               returnvoid
            end ; code
         end ; body
      end ; method
      
