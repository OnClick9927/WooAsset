//
//  AVProVideoBootstrap.m
//  AVPro Video
//
//  Created by Morris Butler on 22/06/2020.
//  Copyright © 2021 RenderHeads. All rights reserved.
//

extern void AVPPluginUnityRegisterRenderingPlugin(void *registerRenderingPluginFunction);

void AVPPluginBootstrap(void)
{
	AVPPluginUnityRegisterRenderingPlugin(UnityRegisterRenderingPluginV5);
}
