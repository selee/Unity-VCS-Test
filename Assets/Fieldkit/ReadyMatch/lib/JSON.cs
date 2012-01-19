using UnityEngine;
using System.Collections;
using System;
using System.Text;

public static class JSON {

	private static object DJgetValue(String input, ref int pos){
		
		///*All Debug statements are commented out*/ //Debug.Log("Found new value, figuring out what it is...");
		
		
		while (input[pos]!='"' && input[pos]!='{' && input[pos]!='[' && ((int)input[pos]<48||(int)input[pos]>57)&&input[pos]!='t'&&input[pos]!='f'&&input[pos]!='n'){
			pos++;
		}
		
		
		
		switch(input[pos]){
		case '"':
			return DJgetString(input,ref pos);
			break;
		case '{':
			return DJgetObject(input,ref pos);
			break;
		case '[':
			return DJgetArray(input,ref pos);
			break;
		case 't':
			if(input.Substring(pos,4).CompareTo("true")==0){
				pos+=4;
				return true;
			}else{
				throw new Exception("Invalid JSON");	
			}
			
			break;
		case 'f':
			if(input.Substring(pos,5).CompareTo("false")==0){
				pos+=5;
				return false;
			}else{
				throw new Exception("Invalid JSON");	
			}
			break;
		case 'n':
			if(input.Substring(pos,4).CompareTo("null")==0){
				pos+=4;
				return null;
			}else{
				throw new Exception("Invalid JSON");	
			}			
			break;
		default:
			return DJgetNumber(input,ref pos);
			break;
		}
	}
	
	private static String DJgetString(String input, ref int pos){
		StringBuilder sb = new StringBuilder();
		while (input[pos]!='"'){
			pos++;
		}
		
		pos++;
		
		///*All Debug statements are commented out*/ //Debug.Log("Found New String!");
		
		
		while(input[pos]!='"'){
			if(input[pos]=='\\'){
				pos++;
				switch(input[pos]){
				case '\"':
					sb.Append('\"');
					break;
				case'\\':
					sb.Append('\\');
					break;
				case'/':
					sb.Append('/');
					break;
				case'b':
					sb.Append('\b');
					break;
				case'f':
					sb.Append('\f');
					break;
				case'n':
					sb.Append('\n');
					break;
				case'r':
					sb.Append('\r');
					break;
				case't':
					sb.Append('\t');
					break;
				case'u':
					pos++;
					String u = Char.ConvertFromUtf32(Convert.ToInt32(input.Substring(pos,4),16));	
					pos+=3;
					sb.Append(u);	
					break;
				}
			}else
				sb.Append(input[pos]);	
			pos++;
		}
		pos++;
		
		///*All Debug statements are commented out*/ //Debug.Log("The string is "+sb.ToString());
		
		
		return sb.ToString();	
	}
	
	private static double DJgetNumber(String input, ref int pos){
		double num = 0;
		bool neg = false;
		while (((int)input[pos]<48 || (int)input[pos]>57) &&input[pos]!='-'){
			pos++;
		}
		
		
		
		///*All Debug statements are commented out*/ //Debug.Log("Found new number!");
		
		if(input[pos]=='-'){
			neg = true;
			pos++;
		}
		
		while((int)input[pos]>=48&&(int)input[pos]<=57){
			num*=10;
			num+=(int)((int)input[pos]-48);
			pos++;
			
		}
		double div = 1.0;
		if(input[pos]=='.'){
			pos++;
			while((int)input[pos]>=48&&(int)input[pos]<=57){
				div*=10.0;
				num+=((int)((int)input[pos]-48))/div;
				pos++;
			}	
		}
		if(input[pos]=='e'||input[pos]=='E'){
			pos++;
			bool negpow = false;
			int pow=0;
			if(input[pos]=='-'){
				negpow=true;
				pos++;
			}
			if(input[pos]=='+')
				pos++;
			while((int)input[pos]>=48&&(int)input[pos]<=57){
				pow*=10;
				pow+=(int)((int)input[pos]-48);
				pos++;
			}
			
			if(negpow)
				pow=-pow;
			num = num*Mathf.Pow(10,pow);			
		}
			
		if(neg)
			num = -num;
		
		
		///*All Debug statements are commented out*/ //Debug.Log("The number is "+num);
		
		return num;
	}
	
	private static Hashtable DJgetObject(String input, ref int pos){
		Hashtable ht = new Hashtable();
		
		while (input[pos]!='{'){
			pos++;
		}
		
		///*All Debug statements are commented out*/ //Debug.Log("Started new Object!");
		pos++;
		
		String name;
		object val;
		while (input[pos]!='"' && input[pos]!='}'){
			pos++;
		}
		
		if(input[pos]!='}')
			pos--;
		else
			return ht;
		
		do{
			pos++;
			name = DJgetString(input,ref pos);
			while (input[pos]!=':'){
				pos++;		
			}
			pos++;
			val = DJgetValue(input,ref pos);
			if(ht.Contains(name))
			   ht[name]=val;
			else
				ht.Add(name,val);
			while (input[pos]!=',' && input[pos]!='}'){
				pos++;		
			}
		}while(input[pos]==',');
		
		pos++;
		return ht;
	}
	
	private static ArrayList DJgetArray(String input, ref int pos){
		ArrayList al = new ArrayList();
		while (input[pos]!='['){
			pos++;
		}
		object val;
		pos++;
		
		while (input[pos]!=']' && input[pos]!='"' && input[pos]!='{' && input[pos]!='[' && ((int)input[pos]<48||(int)input[pos]>57)&&input[pos]!='t'&&input[pos]!='f'&&input[pos]!='n'){
			pos++;
		}
		if(input[pos]!=']')
			pos--;
		else
			return al;
		
		///*All Debug statements are commented out*/ //Debug.Log("Found new Array");
		
		do{
			pos++;
			
			val = DJgetValue(input,ref pos);
			al.Add(val);
			while (input[pos]!=',' && input[pos]!=']'){
				pos++;		
			}
		}while(input[pos]==',');
		
		pos++;
		return al;
	}
	
	
	public static object Decode(String input){
		int i = 0;
		try{
		return DJgetValue(input, ref i);
		}catch(Exception e){
			return null;	
		}
	}
	
	
	private static void EJgetObject(StringBuilder sb, Hashtable ht){
		sb.Append('{');
		bool first = true;

		///*All Debug statements are commented out*/ //Debug.Log(ht.Keys.Count);	
		
		foreach(object k in ht.Keys){
			
			
			///*All Debug statements are commented out*/ //Debug.Log(k.ToString()+' '+k.GetType().ToString());
			
			
			if(first)
				first = false;
			else
				sb.Append(", ");
			EJgetString(sb,(String)k);
			sb.Append(": ");
			EJgetValue(sb,ht[k]);
		}
		sb.Append('}');
		
	}
	private static void EJgetArray(StringBuilder sb, ArrayList al){
		sb.Append('[');
		bool first = true;
		foreach(object i in al){
			if(first)
				first = false;
			else
				sb.Append(", ");

			EJgetValue(sb,i);
		}
		sb.Append(']');
		
	}
	
	private static void EJgetString(StringBuilder sb, String val){
		sb.Append("\"");
		for(var i = 0; i<val.Length; i++){				
			switch(val[i]){
			case '\"':
				sb.Append("\\\"");
				break;
			case '\\':
				if(i<val.Length-1 && val[i+1]=='u')
					sb.Append("\\u");
				else
					sb.Append("\\\\");
				break;
			case '\b':
				sb.Append("\\b");
				break;
			case '\f':
				sb.Append("\\f");
				break;
			case '\n':
				sb.Append("\\n");
				break;
			case '\r':
				sb.Append("\\r");
				break;
			case '\t':
				sb.Append("\\t");
				break;
			default:
				sb.Append(val[i]);
				break;       
			}
		}
		sb.Append("\"");
	}
	
	
	private static void EJgetValue(StringBuilder sb, object val){
		if(val==null){
			sb.Append("null");
			return;
		}
		
		
		///*All Debug statements are commented out*/ //Debug.Log(val.GetType().ToString());
		
		
		
		
		switch(val.GetType().ToString()){
		case "System.String":
			EJgetString(sb,(String)val);
			break;
		case "System.Double":
			sb.Append(val.ToString());	
			break;
		case "System.Collections.Hashtable":
			EJgetObject(sb,(Hashtable)val);	
			break;
		case "System.Collections.ArrayList":
			EJgetArray(sb,(ArrayList)val);
			break;
		case "System.Boolean":	
			if((Boolean)val)
				sb.Append("true");
			else
				sb.Append("false");
			break;
		default:
			sb.Append(val.ToString());
			break;
		}		
	}
	
	
	public static String Encode(object input){
		StringBuilder sb = new StringBuilder();
		EJgetValue(sb,input);
		return sb.ToString();
	}
}
