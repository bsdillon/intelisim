class SimParams:
    '''
    Generically tracks all simulation parameters which may affect agents
    '''
    def __init__(self):
        self.parameters = {}
    
    def add_parameter(self, p_name, p_value):
        '''
        Throws an error if p_name is already defined
        '''
        if p_name in self.parameters:
            raise AttributeError(f"Parameter {p_name} already exists at add")
        self.parameters[p_name]=p_value
    
    def update_parameter(self, p_name, p_value):
        '''
        Throws an error if p_name is NOT already defined
        '''
        if p_name not in self.parameters:
            raise AttributeError(f"Parameter {p_name} does not exist at update")
        self.parameters[p_name]=p_value
    
    def get_parameter(self, p_name):
        '''
        Throws an error if p_name is NOT already defined
        '''
        if p_name not in self.parameters:
            raise AttributeError(f"Parameter {p_name} does not exist at get")
        return self.parameters[p_name]