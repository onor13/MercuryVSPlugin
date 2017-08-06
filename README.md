# Visual Studio Plugin for Mercury Language. 

### Prerequisite:
Visual Studio 2017, might works with older versions, didn’t try it.

### Features:
1)	Syntax coloring + same word highlight.  The colors can be changed from Visual Studio “Font and Colors” options, except for the highlight same word.
  Colors mapping is the following:
*	Mercury Comment: Comment
*	Brace matching: Brace matching
*	String literal: String
*	Mercury variable: C++ macros
*	Keyword: keyword

2)	Go to definition
3)	Find all references: in order to use this functionality you need to open the Output window manually: View -> Output
4)	Auto-completion: uses tokens from current module and declarations from imported modules.
Most of them are approximate, due to the fact that I don’t a have a full Mercury parser.
### Configuration:
In order to be able to profit from this plugin functionalities, 
you need to set up the configuration from Visual Studio options like this:
![vspluginoptions](https://user-images.githubusercontent.com/19971537/29004723-b4929d04-7acc-11e7-9608-1f950db257c9.JPG)

### Screenshots:
Find all references result:
![vspluginfindallreferences](https://user-images.githubusercontent.com/19971537/29004732-d9316c12-7acc-11e7-800b-e7d77bda12e6.JPG)
