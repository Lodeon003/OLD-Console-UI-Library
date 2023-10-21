using Lodeon.Terminal.UI;
using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Paging;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

/*  TODO:

-HIGH PRIORITY-

    [_] Hide output driver from Element objects, add support for display functions that take in element's graphic buffer and merge it
        . with all parent and child buffers

    [_] Impelent page changing program and make it so it can be called by elements and pages. Add other methods
        . Make abstract Main() and OnExit() methods on Page that will be called by the program when page changes

    [_] Implement system to return array of expeptions from Page's and Element's Initialize functions and have an overridable method - Page OnInitializeException(ExceptionCollection exceptions)
        . in Program class that prints errors in a custom error page by default. Method must return a custom page to display in case of errors.
        . The ExceptionCollection must have an internal Add method and public iterator. That collection should be given by the script so not to create many objects. Program could just check after everything is initialized if Exceptions.Count == 0
    
    [_] Make sure page and program methods are not exposed. Create navigator class that allows elements and pages to notify program when to change pages.
        . OnNavigate<TValue>(), Next(), Previous(), Navigate(TKey), Navigate(Value), Values.
 
    [#] Make it so parent elements (only Containers) have childen, can have them added viewed and removed, and have to rearrange them based on a "LayoutHandler"
        . if itself or it's parent changes size or position

    [#] Change the element's "OnDraw" function with other events and make the "Canvas" property visible to subclasses so it can be drawn when needed
        . Need to define a method to actually display what was drawn on the canvas

    [#] Decide how to go about focusing policy. Make it so elements can be hovered or focused. If no element is focused you can use arrow keys to hover on element's which have 'IsFocusable' true (if can't focus no point in hovering it so just skip it)
        . You start from the page container which will let you hover it's children. If you hover on a container and press ENTER you will hover it's children. If you press ENTER on a non-container you will focus
        . that item and interact with it. If you press ESC while focusing an item you lose focus and hover it's container children. If you press ESC while hovering a container's content you will go back and hover it's parent's children
        . Add a 'PassThrough' property for containers that specify whether it's children are focusable or not. If 'true' it will behave as a container. It's children will be hovered and focused.
        . If 'false' it will behave like a normal element. The whole element will be hovered and it may be focused if 'IsFocusable' is true.
        . Make it so input is first handled by the script, then page, then elements. This way navigation will be handled on a Page level.
        . <should TreeNavigator be accessible to all elements?>

    [#] In Driver.cs implement system to clamp values and colors if 'AllowOutOfBounds' and 'AllowTransparentColors' and throw exceptions if not

    [#] Add OnFocus and OnLostFocus events for elements. Add method Unfocus() on elements class

    [=] LayoutElement TreeFromXML with attribute for naming

    [_] Implement event on Driver class that fires whenever output window's size changes. Not sure on how to force it

    [_] Layout property change updates graphics and recalculates LayoutResults
    
    [#] Event based input system for Element objects. EVent Propagation: (Driver > Program > Page > Selected Element)
        . Add event for keyboard input in driver class
    
    [#] Implement element focusing and element navigation [<- previous item, -> next item, [_] move to children, [ESC] exit to parent
        . Add abstract properties 'IsFocusable' and 'IsContainer'. Container items can have children and if focused they will automatically browse children
        . Wrap in a Browser class to reduce clutter. Create interface IBrowsable<TChildren, TParent> with methods to retrieve family members
   
    [#] Layout stacking algorithm

-LOW PRIORITY-

    [-] Add reference to current Program in Element and Page class so they can access information like program's name.
        . May be better to wrap in a ProgramInfo class
    
    [#] Implement methods to get items in a page by type, by id or both

    [-] Add functions to draw shapes on grahpic buffers using source coordinates and/or precentages. <Maybe use wrapper to prevent
        . buffer size and position change>
 */

return;
RootElement root = LayoutElement.TreeFromXml("Square.xml", null, null, null);
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

class MainPage : Lodeon.Terminal.UI.Layout.LayoutPage
{
    protected override void OnSelect()
    {
        
    }

    protected override void OnDeselect()
    {
        
    }
}
