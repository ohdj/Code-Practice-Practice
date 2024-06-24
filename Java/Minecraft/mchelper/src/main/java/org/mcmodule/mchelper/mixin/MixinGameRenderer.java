package org.mcmodule.mchelper.mixin;

import net.minecraft.client.renderer.GameRenderer;
import org.mcmodule.mchelper.McHelper;
import org.mcmodule.mchelper.event.events.Render2DEvent;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;

@Mixin(GameRenderer.class)
public class MixinGameRenderer {
    @Inject(method = "render",at = @At(value = "INVOKE", target = "Lnet/minecraft/client/gui/Gui;render(Lcom/mojang/blaze3d/vertex/PoseStack;F)V"))
    public void onRender(float p_109094_, long p_109095_, boolean p_109096_, CallbackInfo ci){
        Render2DEvent event = new Render2DEvent();
        McHelper.getInstance().getEventManager().call(event);
    }
}
