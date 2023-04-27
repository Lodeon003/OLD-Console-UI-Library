using Lodeon.Terminal.UI;
using Lodeon.Terminal.UI.Layout;

/*  TODO:

-HIGH PRIORITY-

    [_] Hide output driver from Element objects, add support for display functions that take in element's graphic buffer and merge it
        . with all parent and child buffers

    [_] Impelent page changing program and make it so it can be called by elements and pages. Add other methods
        . Make abstract Main() and OnExit() methods on Page that will be called by the program when page changes

    [#] Fix layout element Update method as having all children update in the "Update method" of one of them is bad idea.
        If all children get updated, every children updates every children

    [=] LayoutElement TreeFromXML with attribute for naming

    [#] Implement event on Driver class that fires whenever output window's size changes. Not sure on how to force it

    [_] Layout property change updates graphics and recalculates LayoutResults
        
    
    [#] Event based input system for Element objects. EVent Propagation: (Driver > Program > Page > Selected Element)
        . Add event for keyboard input in driver class
    
    [#] Implement element focusing and element navigation [<- previous item, -> next item, [_] move to children, [ESC] exit to parent
        . Add OnFocus and OnLostFocus events for elements.
    
    
    [#] Layout stacking algorithm

-LOW PRIORITY-

    [-] Add reference to current Program in Element and Page class so they can access information like program's name.
        . May be better to wrap in a ProgramInfo class
    
    [#] Implement methods to get items in a page by type, by id or both

    [-] Add functions to draw shapes on grahpic buffers using source coordinates and/or precentages. <Maybe use wrapper to prevent
        . buffer size and position change>
 */
await Script.Run<MyProgram>();

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