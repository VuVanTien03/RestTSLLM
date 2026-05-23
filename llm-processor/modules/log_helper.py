# log_helper.py

class LogHelper:
    
    debug = False
    
    def write(context, message_type, message):
        if (LogHelper.debug == False and message_type == "Debug"):
            return
        
        print(f'[{context}][{message_type}] {message}')