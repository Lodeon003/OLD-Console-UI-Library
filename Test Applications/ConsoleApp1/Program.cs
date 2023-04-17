using Lodeon.Terminal.UI;
using Lodeon.Terminal.UI.Layout;

/*  TODO:
    [#] Layout stacking algorithm

    [#] Layout property change updates graphics and recalculates LayoutResults

    [!] <Hide output driver from Element objects>, add support for display functions that take in element's graphic buffer and merge it
        . with all parent and child buffers

    [#] LayoutElement TreeFromXML with attribute for naming

    [#] Event based input system for Element objects. EVent Propagation: (Program > Page > Selected Element)
        . allow for returning a bool in input callback to stop inputs from propagating to page or items

    [!] Add functions to draw shapes on grahpic buffers using source coordinates and/or precentages. <Maybe use wrapper to prevent
        . buffer size and position change>

    [#] Implement event on Driver class that fires whenever output window's size changes. Not sure on how to force it
    
    [#] Add reference to current Program in Element and Page class so they can access information like program's name.
        . May be better to wrap in a ProgramInfo class

    [#] Impelent page changing program and make it so it can be called by elements and pages.
        . Make abstract Main() and OnExit() methods on Page that will be called by the program when page changes

    [#] Implement methods to get items in a page by type, by id or both
 */

await Script.Run<MyProgram>();

class MyProgram : Script
{
    protected override void OnInitialize(PageInitializer Pages)
    {
        Pages.Add<MainPage>("Main", true);
    }

    protected override void Main()
    {
        Console.WriteLine("progrma");
    }
}

class MainPage : LayoutPage
{
        
}