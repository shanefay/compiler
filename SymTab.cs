using System;
 
namespace Tastier { 

public class Obj { // properties of declared symbol
   public string name; // its name
   public int kind;    // var, proc or scope
   public int type;    // its type if var (undef for proc)
   public int sort;

   public int level;   // lexic level: 0 = global; >= 1 local
   public int size;
   public int adr;     // address (displacement) in scope 
   public Obj next;    // ptr to next object in scope
   // for scopes
   public Obj outer;   // ptr to enclosing scope
   public Obj locals;  // ptr to locally declared objects
   public int nextAdr; // next free address in scope
   //default assigned value is false, this is used for constants
   public bool assigned = false;

   //for procedures, keep track of the number of paremeters
   public int paramCount;
}

public class SymbolTable {

   const int // object kinds
      var = 0, proc = 1, scope = 2, constant = 3; 

   const int // types
      undef = 0, integer = 1, boolean = 2;

   const int // sorts
       scalar = 1, array = 2;    

   public Obj topScope; // topmost procedure scope
   public int curLevel; // nesting level of current scope
   public Obj undefObj; // object node for erroneous symbols

   public bool mainPresent;
   
   Parser parser;
   
   public SymbolTable(Parser parser) {
      curLevel = -1; 
      topScope = null;
      undefObj = new Obj();
      undefObj.name = "undef";
      undefObj.kind = var;
      undefObj.type = undef;
      undefObj.sort = scalar;
      undefObj.level = 0;
      undefObj.size = 0;
      undefObj.adr = 0;
      undefObj.next = null;
      undefObj.assigned = false;
      undefObj.paramCount = 0;
      this.parser = parser;
      mainPresent = false;

   }

// open new scope and make it the current scope (topScope)
   public void OpenScope() {
      Obj scop = new Obj();
      scop.name = "";
      scop.kind = scope; 
      scop.outer = topScope; 
      scop.locals = null;
      scop.nextAdr = 0;
      topScope = scop; 
      curLevel++;
   }

// close current scope
   public void CloseScope() {
      Obj item = topScope.locals;
      int type, kind, level;
      string typeName, kindName, levelDescription;
      while(item != null){
         type = item.type;
         kind = item.kind;
         level = item.level;

         if(type ==0){
            typeName = "undefined";
         } else if(type == 1){
            typeName = "integer";
         } else {
            typeName = "boolean";
         }

         if(kind ==0){
            kindName = "var";
         } else if(kind ==1){
            kindName ="proc";
         } else if(kind == 3) {
            kindName = "constant";
         }else {
            kindName ="scope";
         }

         if(level == 0){
              levelDescription = "global"; 
         } else {
            levelDescription = "local";
         }
         Console.WriteLine(";Name: {0}, Type: {1}, Kind: {2}, Assigned {4}, Level: {3}", item.name, typeName, kindName, levelDescription, item.assigned);

         item = item.next;
      }
      topScope = topScope.outer;
      curLevel--;
   }

// open new sub-scope and make it the current scope (topScope)
   public void OpenSubScope() {
   // lexic level remains unchanged
      Obj scop = new Obj();
      scop.name = "";
      scop.kind = scope;
      scop.outer = topScope;
      scop.locals = null;
   // next available address in stack frame remains unchanged
      scop.nextAdr = topScope.nextAdr;
      topScope = scop;
   }

// close current sub-scope
   public void CloseSubScope() {
   // lexic level remains unchanged
      topScope = topScope.outer;
   }

// create new object node in current scope
   //added paremeters for a parameter count and and address for parameters
   //size added for arrays. its used to space out relative addresses and therefore actual addressed 
   //for objects larger than size 1
   public Obj NewObj(string name, int kind, int type, int sort, int size, int adr, int parameters) {
      Obj p, last; 
      Obj obj = new Obj();
      obj.name = name; obj.kind = kind;
      obj.type = type; obj.level = curLevel; 
      obj.sort = sort;
      obj.next = null; 
      obj.size = size;
      obj.paramCount = parameters;

      obj.next = null;
      p = topScope.locals; last = null;
      while (p != null) { 
         if (p.name == name)
            parser.SemErr("name declared twice");
         last = p; p = p.next;
      }
      if (last == null)
         topScope.locals = obj; else last.next = obj;
      if (kind == var || kind == constant){
         if(adr <= -5)
          obj.adr = adr;
        else
          obj.adr = topScope.nextAdr + size;
      }    
      return obj;
   }

// search for name in open scopes and return its object node
   public Obj Find(string name) {
      Obj obj, scope;
      scope = topScope;
      while (scope != null) { // for all open scopes
         obj = scope.locals;
         while (obj != null) { // for all objects in this scope
            if (obj.name == name) return obj;
            obj = obj.next;
         }
         scope = scope.outer;
      }
      parser.SemErr(name + " is undeclared");
      return undefObj;
   }

} // end SymbolTable

} // end namespace
