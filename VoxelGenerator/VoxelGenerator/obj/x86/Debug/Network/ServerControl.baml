   M S B A M L   `   `   `  ����I  EVoxelGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:VoxelGenerator.NetworkVoxelGenerator.Network  5      *    $VoxelGenerator.Network.ServerControl   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      6local$clr-namespace:VoxelGenerator.Network     5      $	��300��5      $	��350��6   $	��350��6'   $	��300��66     � Title$  Server��6F   .��5	      � .+�5
   
   �� -   5         ��Name$ 
tabControl��5
      .@�5      �� $��Server Info��6   .��5      � .+�5      �� -   5         ��Name$ label��5      $��	Message: ��6+   $
��Left=�6?   $��	20,50,0,0q�6Z   $	��Top=�6m   $��26��6�   �� 5      -   5         ��Name$ serverMessageTextBox��5      $��
90,53,20,0q�6<    �� TextWrapping$
 Wrap=�6P   $��A Server��6d   $	��Top=�6t   $��Segoe UI Light)�6�   ���	#FFACACAC$��23��6�    �� 	IsEnabled . 6�   �� 5      2  ,System.Windows.Markup.XmlAttributeProperties  XmlSpace preserve6�   -   5         ��Name$ ipLabel��5       $��
20,15,20,0q�61   $	��Top=�6E   ���	#FF787878$��35��6t   $
��Wrap=�6�   DBLocal IP: 127.0.0.1
External IP: 0.0.0.0:0000
                    5      �� 5      -   5        $ playerCountLabel��5      $��0 players online��66   $
��Left=�6Q   $	��Top=�6l   $��	20,80,0,0q�6�   5      5      �� 5      $��Map Info��6   .��5      � .+�5      � -   5         �Name$ mapGrid��5   /   }�. $��20,100,20,20q�6@   ���	#FFECECEC$��Disabled=�6�   	 � ContextMenu	 5      �� 5      .@�5   "   m� -   5        $��
Center Map��5   +   6?   5      5      �� 5      -   5        $ chunkLoadLabel��5      $��0 chunks loaded��64   $
��Left=�6N   $	��Top=�6i   $��	20,20,0,0q�6�   5       5!      �� 5"      $��Administration��6   .��5#      � 5$      5%      �� 5&      $��Server Diagnostics��6   .��5'      � 5(      5)      5*      5+      5,      