using System;
using System.Collections.Generic;
using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.VertexSource;
using MatterHackers.Agg.UI;

using MatterHackers.Agg.Transform;

using Gaming.Math;
using Gaming.Game;
using Gaming.Graphics;

namespace RockBlaster
{
    /// <summary>
    /// Description of HowManyPlayersMenu.
    /// </summary>
    public class HowManyPlayersMenu : GuiWidget
    {
        public delegate void StartOnePlayerGameEventHandler(GuiWidget button);
        public event StartOnePlayerGameEventHandler StartOnePlayerGame;

        public delegate void StartTwoPlayerGameEventHandler(GuiWidget button);
        public event StartTwoPlayerGameEventHandler StartTwoPlayerGame;

        public delegate void StartFourPlayerGameEventHandler(GuiWidget button);
        public event StartFourPlayerGameEventHandler StartFourPlayerGame;

        public delegate void CancelMenuEventHandler(GuiWidget button);
        public event CancelMenuEventHandler CancelMenu;

        public HowManyPlayersMenu(RectangleDouble bounds)
        {
            BoundsRelativeToParent = bounds;
            ImageSequence onePlayerButtonSequence = (ImageSequence)DataAssetCache.Instance.GetAsset(typeof(ImageSequence), "OnePlayerButton");
            Button onePlayerGameButton = new Button(270, 310, new ButtonViewThreeImage(onePlayerButtonSequence.GetImageByIndex(0), onePlayerButtonSequence.GetImageByIndex(1), onePlayerButtonSequence.GetImageByIndex(2)));
            AddChild(onePlayerGameButton);
            onePlayerGameButton.Click += new Button.ButtonEventHandler(OnStartOnePlayerGameButton);

            ImageSequence twoPlayerButtonSequence = (ImageSequence)DataAssetCache.Instance.GetAsset(typeof(ImageSequence), "TwoPlayerButton");
            Button twoPlayerGameButton = new Button(400, 310, new ButtonViewThreeImage(twoPlayerButtonSequence.GetImageByIndex(0), twoPlayerButtonSequence.GetImageByIndex(1), twoPlayerButtonSequence.GetImageByIndex(2)));
            AddChild(twoPlayerGameButton);
            twoPlayerGameButton.Click += new Button.ButtonEventHandler(OnStartTwoPlayerGameButton);

            ImageSequence fourPlayerButtonSequence = (ImageSequence)DataAssetCache.Instance.GetAsset(typeof(ImageSequence), "FourPlayerButton");
            Button fourPlayerGameButton = new Button(530, 310, new ButtonViewThreeImage(fourPlayerButtonSequence.GetImageByIndex(0), fourPlayerButtonSequence.GetImageByIndex(1), fourPlayerButtonSequence.GetImageByIndex(2)));
            AddChild(fourPlayerGameButton);
            fourPlayerGameButton.Click += new Button.ButtonEventHandler(OnStartFourPlayerGameButton);

            ImageSequence cancelButtonSequence = (ImageSequence)DataAssetCache.Instance.GetAsset(typeof(ImageSequence), "NumPlayersCancelButton");
            Button cancelGameButton = new Button(400, 210, new ButtonViewThreeImage(cancelButtonSequence.GetImageByIndex(0), cancelButtonSequence.GetImageByIndex(1), cancelButtonSequence.GetImageByIndex(2)));
            AddChild(cancelGameButton);
            cancelGameButton.Click += new Button.ButtonEventHandler(OnCancelMenuButton);
        }

        public override void OnDraw(Graphics2D graphics2D)
        {
            ImageSequence menuBackground = (ImageSequence)DataAssetCache.Instance.GetAsset(typeof(ImageSequence), "NumPlayersSelectBackground");
            graphics2D.Render(menuBackground.GetImageByIndex(0), 0, 0);

            base.OnDraw(graphics2D);
        }

        private void OnStartOnePlayerGameButton(object sender, MouseEventArgs mouseEvent)
        {
            if (StartOnePlayerGame != null)
            {
                StartOnePlayerGame(this);
            }
        }

        private void OnStartTwoPlayerGameButton(object sender, MouseEventArgs mouseEvent)
        {
            if (StartTwoPlayerGame != null)
            {
                StartTwoPlayerGame(this);
            }
        }

        private void OnStartFourPlayerGameButton(object sender, MouseEventArgs mouseEvent)
        {
            if (StartFourPlayerGame != null)
            {
                StartFourPlayerGame(this);
            }
        }

        private void OnCancelMenuButton(object sender, MouseEventArgs mouseEvent)
        {
            if (CancelMenu != null)
            {
                CancelMenu(this);
            }
        }
    }
}