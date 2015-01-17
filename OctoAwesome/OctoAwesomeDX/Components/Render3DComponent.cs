﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OctoAwesome.Components
{
    internal sealed class Render3DComponent : DrawableGameComponent
    {
        private WorldComponent world;
        private Camera3DComponent camera;

        private BasicEffect effect;
        private Texture2D grass;
        private Texture2D sand;
        private Texture2D tree;

        private VertexBuffer vb;
        private IndexBuffer ib;
        private int vertexCount;
        private int indexCount;

        public Render3DComponent(Game game, WorldComponent world, Camera3DComponent camera)
            : base(game)
        {
            this.world = world;
            this.camera = camera;
        }

        protected override void LoadContent()
        {
            int width = world.World.Map.CellCache.GetLength(0);
            int height = world.World.Map.CellCache.GetLength(1);
            vertexCount = width * height * 4;
            indexCount = width * height * 6;

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[vertexCount];
            short[] index = new short[indexCount];

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int vertexOffset = (((z * width) + x) * 4);
                    int indexOffset = (((z * width) + x) * 6);

                    vertices[vertexOffset + 0] = new VertexPositionNormalTexture(new Vector3(x, 0, z), Vector3.Up, new Vector2(0, 0));
                    vertices[vertexOffset + 1] = new VertexPositionNormalTexture(new Vector3(x + 1, 0, z), Vector3.Up, new Vector2(1, 0));
                    vertices[vertexOffset + 2] = new VertexPositionNormalTexture(new Vector3(x, 0, z + 1), Vector3.Up, new Vector2(0, 1));
                    vertices[vertexOffset + 3] = new VertexPositionNormalTexture(new Vector3(x + 1, 0, z + 1), Vector3.Up, new Vector2(1, 1));

                    index[indexOffset + 0] = (short)(vertexOffset + 0);
                    index[indexOffset + 1] = (short)(vertexOffset + 1);
                    index[indexOffset + 2] = (short)(vertexOffset + 3);
                    index[indexOffset + 3] = (short)(vertexOffset + 0);
                    index[indexOffset + 4] = (short)(vertexOffset + 3);
                    index[indexOffset + 5] = (short)(vertexOffset + 2);
                }
            }

            vb = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertexCount, BufferUsage.WriteOnly);
            vb.SetData<VertexPositionNormalTexture>(vertices);

            ib = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);
            ib.SetData<short>(index);

            grass = Game.Content.Load<Texture2D>("Textures/grass_center");
            sand = Game.Content.Load<Texture2D>("Textures/sand_center");
            tree = Game.Content.Load<Texture2D>("Textures/tree");

            effect = new BasicEffect(GraphicsDevice);
            effect.World = Matrix.Identity;
            effect.Projection = camera.Projection;
            effect.TextureEnabled = true;

            // effect.EnableDefaultLighting();
            //effect.LightingEnabled = true;
            //effect.AmbientLightColor = Color.DarkGray.ToVector3();

            //effect.DirectionalLight0.Enabled = true;
            //effect.DirectionalLight0.Direction = new Vector3(-3, -3, -5);
            //effect.DirectionalLight0.DiffuseColor = Color.Red.ToVector3();


            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // GraphicsDevice.RasterizerState.CullMode = CullMode.None;
            // GraphicsDevice.RasterizerState.FillMode = FillMode.WireFrame;

            effect.World = Matrix.Identity;
            effect.View = camera.View;
            GraphicsDevice.SetVertexBuffer(vb);
            GraphicsDevice.Indices = ib;

            int width = world.World.Map.CellCache.GetLength(0);
            int height = world.World.Map.CellCache.GetLength(1);
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    OctoAwesome.Model.CellCache cell = world.World.Map.CellCache[x, z];

                    switch (cell.CellType)
                    {
                        case Model.CellType.Gras:
                            effect.Texture = grass;
                            break;
                        case Model.CellType.Sand:
                            effect.Texture = sand;
                            break;
                    }

                    int indexOffset = ((z * width) + x) * 6;

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, indexOffset, 2);
                    }
                }
            }

            VertexPositionNormalTexture[] localVertices = new VertexPositionNormalTexture[] 
            {
                new VertexPositionNormalTexture(new Vector3(-0.5f, 1, 0), Vector3.Backward, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(0.5f, 1, 0), Vector3.Backward, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(0.5f, 0, 0), Vector3.Backward, new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-0.5f, 1, 0), Vector3.Backward, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(0.5f, 0, 0), Vector3.Backward, new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-0.5f, 0, 0), Vector3.Backward, new Vector2(0, 1)),
            };

            foreach (var item in world.World.Map.Items.OrderBy(t => t.Position.Y))
            {
                if (item is OctoAwesome.Model.TreeItem)
                {
                    effect.Texture = tree;

                    localVertices[0].Position = new Vector3(-0.5f, 2, 0);
                    localVertices[1].Position = new Vector3(0.5f, 2, 0);
                    localVertices[3].Position = new Vector3(-0.5f, 2, 0);

                    effect.World = Matrix.CreateTranslation(item.Position.X + 0.5f, 0, item.Position.Y + 0.5f);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, localVertices, 0, 2);
                    }

                    //spriteBatch.Draw(tree, new Rectangle(
                    //                (int)(item.Position.X * camera.SCALE - camera.ViewPort.X) - 30,
                    //                (int)(item.Position.Y * camera.SCALE - camera.ViewPort.Y) - 118,
                    //                (int)camera.SCALE,
                    //                (int)camera.SCALE * 2), Color.White);
                }

                if (item is OctoAwesome.Model.BoxItem)
                {
                    //spriteBatch.Draw(box, new Rectangle(
                    //                (int)(item.Position.X * camera.SCALE - camera.ViewPort.X) - 32,
                    //                (int)(item.Position.Y * camera.SCALE - camera.ViewPort.Y) - 35,
                    //                (int)camera.SCALE,
                    //                (int)camera.SCALE), Color.White);
                }

                if (item is OctoAwesome.Model.Player)
                {
                    //int frame = (int)((gameTime.TotalGameTime.TotalMilliseconds / 250) % 4);

                    //int offsetx = 0;
                    //if (world.World.Player.State == OctoAwesome.Model.PlayerState.Walk)
                    //{
                    //    switch (frame)
                    //    {
                    //        case 0: offsetx = 0; break;
                    //        case 1: offsetx = SPRITE_WIDTH; break;
                    //        case 2: offsetx = 2 * SPRITE_WIDTH; break;
                    //        case 3: offsetx = SPRITE_WIDTH; break;
                    //    }
                    //}
                    //else
                    //{
                    //    offsetx = SPRITE_WIDTH;
                    //}

                    //// Umrechung in Grad
                    //float direction = (world.World.Player.Angle * 360f) / (float)(2 * Math.PI);

                    //// In positiven Bereich
                    //direction += 180;

                    //// Offset
                    //direction += 45;

                    //int sector = (int)(direction / 90);

                    //int offsety = 0;
                    //switch (sector)
                    //{
                    //    case 1: offsety = 3 * SPRITE_HEIGHT; break;
                    //    case 2: offsety = 2 * SPRITE_HEIGHT; break;
                    //    case 3: offsety = 0 * SPRITE_HEIGHT; break;
                    //    case 4: offsety = 1 * SPRITE_HEIGHT; break;
                    //}

                    //Point spriteCenter = new Point(27, 48);

                    //spriteBatch.Draw(sprite,
                    //    new Rectangle(
                    //        ((int)(world.World.Player.Position.X * camera.SCALE) - camera.ViewPort.X - spriteCenter.X),
                    //        ((int)(world.World.Player.Position.Y * camera.SCALE) - camera.ViewPort.Y - spriteCenter.Y), SPRITE_WIDTH, SPRITE_HEIGHT),
                    //    new Rectangle(offsetx, offsety, SPRITE_WIDTH, SPRITE_HEIGHT),
                    //    Color.White);
                }
            }
        }
    }
}
