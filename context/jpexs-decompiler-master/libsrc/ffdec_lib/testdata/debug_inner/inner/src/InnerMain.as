package
{
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.text.TextField;
	import flash.text.TextFormat;
		
	public class InnerMain extends Sprite 
	{
		MyInnerClass;
		
		public function InnerMain() 
		{
			if (stage) init();
			else addEventListener(Event.ADDED_TO_STAGE, init);
		}
		
		private function init(e:Event = null):void
		{
			removeEventListener(Event.ADDED_TO_STAGE, init);
			
			var textField:TextField = new TextField();

			textField.text = "Main inner1.";

			var textFormat:TextFormat = new TextFormat();
			textFormat.size = 24;
			textFormat.color = 0x000000;
			textField.setTextFormat(textFormat);
			textField.width = 200;
						
			addChild(textField);

			textField.x = 50;
			textField.y = 125;
		}
		
	}
	
}