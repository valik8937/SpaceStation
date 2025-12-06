using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceStation.Content.Components;

namespace SpaceStation.Client.Graphics;

/// <summary>
/// System for rendering sprites to the screen.
/// </summary>
public sealed class RenderSystem
{
    private static readonly QueryDescription SpriteQuery = new QueryDescription()
        .WithAll<Transform, Sprite>();
        
    private static readonly QueryDescription AnimatedQuery = new QueryDescription()
        .WithAll<Transform, AnimatedSprite>();
    
    /// <summary>
    /// Draws all entities with sprite components.
    /// </summary>
    public void Draw(World world, SpriteBatch spriteBatch, Camera camera, Viewport viewport)
    {
        spriteBatch.Begin(
            SpriteSortMode.BackToFront,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null,
            null,
            null,
            camera.GetTransform(viewport)
        );
        
        // Draw static sprites
        world.Query(in SpriteQuery, (ref Transform transform, ref Sprite sprite) =>
        {
            if (sprite.Texture == null)
                return;
                
            var position = new Vector2(transform.Position.X * 32, transform.Position.Y * 32);
            var sourceRect = sprite.SourceRect == Rectangle.Empty 
                ? null as Rectangle?
                : sprite.SourceRect;
                
            spriteBatch.Draw(
                sprite.Texture,
                position,
                sourceRect,
                sprite.Tint,
                transform.Rotation,
                Vector2.Zero,
                sprite.Scale,
                SpriteEffects.None,
                sprite.LayerDepth
            );
        });
        
        // Draw animated sprites
        world.Query(in AnimatedQuery, (ref Transform transform, ref AnimatedSprite anim) =>
        {
            if (anim.Texture == null)
                return;
                
            var position = new Vector2(transform.Position.X * 32, transform.Position.Y * 32);
            
            spriteBatch.Draw(
                anim.Texture,
                position,
                anim.CurrentSourceRect,
                Color.White,
                transform.Rotation,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0.5f
            );
        });
        
        spriteBatch.End();
    }
    
    /// <summary>
    /// Updates animated sprites.
    /// </summary>
    public void UpdateAnimations(World world, float deltaTime)
    {
        world.Query(in AnimatedQuery, (ref AnimatedSprite anim) =>
        {
            anim.TimeAccumulator += deltaTime;
            
            if (anim.TimeAccumulator >= anim.FrameTime)
            {
                anim.TimeAccumulator -= anim.FrameTime;
                anim.CurrentFrame++;
                
                if (anim.CurrentFrame >= anim.TotalFrames)
                {
                    anim.CurrentFrame = anim.Loop ? 0 : anim.TotalFrames - 1;
                }
            }
        });
    }
}
