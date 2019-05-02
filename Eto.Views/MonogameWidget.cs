using System;
using Gtk;
using Microsoft.Xna.Framework;

namespace gtkView
{
    public class MonogameWidget<T> : Box where T : Game, new()
    {
        public T Game { get; private set; }

        public MonogameWidget()
            : base(Orientation.Horizontal, 0)
        {
            Destroyed += MonogameWidget_Destroyed;
        }

        public void Run()
        {
            var dummyContext = new GLArea();
            
            PackStart(dummyContext, true, true, 0);
            if(Children.Length == 0)
                throw new Exception("No dummy context added");

            var context = Children[0] as GLArea;
            if(context == null)
                throw new Exception($"First child is not of type {nameof(GLArea)}");
            
            context.Context.MakeCurrent();
            
            Game = new T();
            var eventArea = Game.Services.GetService<Widget>();
            eventArea.Expand = true;
            
            PackEnd(eventArea, true, true, 0);
            
            ShowAll();

            Game.Run();

            Remove(context);
            ShowAll();
        }

        private void MonogameWidget_Destroyed(object sender, EventArgs e)
        {
            Game?.Exit();
        }
    }
}