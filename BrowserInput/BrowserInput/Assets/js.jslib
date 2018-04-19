mergeInto(LibraryManager.library, {

	CallGyroSensor: function()
	{
		window.alert("GyroCall test");	
	}
	GetSensorInput: function(str)
	{
		window.alert(Pointer_stringify(str));		
	}
	PrintFloatArray: function (array, size) 
	{
		for(var i = 0; i < size; i++)
		console.log(HEAPF32[(array >> 2) + i]);
	},
 
	
});