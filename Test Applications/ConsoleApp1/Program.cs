using Lodeon.Terminal.UI;
using Lodeon.Terminal.UI.Layout;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

/*  TODO:

-HIGH PRIORITY-

    [_] Hide output driver from Element objects, add support for display functions that take in element's graphic buffer and merge it
        . with all parent and child buffers

    [_] Impelent page changing program and make it so it can be called by elements and pages. Add other methods
        . Make abstract Main() and OnExit() methods on Page that will be called by the program when page changes

    [#] Implement system to return array of expeptions from Page's and Element's Initialize functions and have an overridable method - Page OnInitializeException(ExceptionCollection exceptions)
        . in Program class that prints errors in a custom error page by default. Method must return a custom page to display in case of errors.
        . The ExceptionCollection must have an internal Add method and public iterator. That collection should be given by the script so not to create many objects. Program could just check after everything is initialized if Exceptions.Count == 0

    [#] Fix layout element Update method as having all children update in the "Update method" of one of them is bad idea.
        If all children get updated, every children updates every children

    [=] LayoutElement TreeFromXML with attribute for naming

    [#] Implement event on Driver class that fires whenever output window's size changes. Not sure on how to force it

    [_] Layout property change updates graphics and recalculates LayoutResults
        
    
    [#] Event based input system for Element objects. EVent Propagation: (Driver > Program > Page > Selected Element)
        . Add event for keyboard input in driver class
    
    [#] Implement element focusing and element navigation [<- previous item, -> next item, [_] move to children, [ESC] exit to parent
        . Add abstract properties 'IsFocusable' and 'IsContainer'. Container items can have children and if focused they will automatically browse children
        . Wrap in a Browser class to reduce clutter. Create interface IBrowsable<TChildren, TParent> with methods to retrieve family members
   
    [#] Add OnFocus and OnLostFocus events for elements. Add method Throw(Exception e, bool stopExecution) and Unfocus() on elements class
    
    [#] Layout stacking algorithm

-LOW PRIORITY-

    [-] Add reference to current Program in Element and Page class so they can access information like program's name.
        . May be better to wrap in a ProgramInfo class
    
    [#] Implement methods to get items in a page by type, by id or both

    [-] Add functions to draw shapes on grahpic buffers using source coordinates and/or precentages. <Maybe use wrapper to prevent
        . buffer size and position change>
 */

return;
RootElement root = LayoutElement.TreeFromXml("Square.xml", null);
return;
//await Script.Run<MyProgram>();

class MyProgram : Script
{
    protected override void OnInitialize(PageInitializer Pages)
    {
        Pages.AddMain<MainPage>("Main");
    }

    protected override void Main()
    {
        Console.WriteLine("progrma");
    }
}

class MainPage : LayoutPage
{
    protected override void OnSelect()
    {
        
    }

    protected override void OnDeselect()
    {
        
    }
}
